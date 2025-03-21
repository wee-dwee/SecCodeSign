using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

class GPGSigner
{
    static void SignFile(string filePath, string keyID)
    {
        string signatureFile = filePath + ".sig";

        Console.WriteLine($"Signing file using key: {keyID}...");

        try
        {
            Process process = new Process();
            process.StartInfo.FileName = "gpg";
            // ✅ Add passphrase if available as an environment variable
            string? passphrase = Environment.GetEnvironmentVariable("GPG_PASSPHRASE");
            if (!string.IsNullOrWhiteSpace(passphrase))
            {
                process.StartInfo.ArgumentList.Add("--passphrase");
                process.StartInfo.ArgumentList.Add(passphrase);
            }

            // ✅ Add --batch and --pinentry-mode loopback to avoid interactive input
            process.StartInfo.ArgumentList.Add("--batch");
            process.StartInfo.ArgumentList.Add("--yes");
            process.StartInfo.ArgumentList.Add("--pinentry-mode");
            process.StartInfo.ArgumentList.Add("loopback");

            // ✅ GPG Signing Arguments
            process.StartInfo.ArgumentList.Add("-u");
            process.StartInfo.ArgumentList.Add(keyID);
            process.StartInfo.ArgumentList.Add("--detach-sign");
            process.StartInfo.ArgumentList.Add("--armor");
            process.StartInfo.ArgumentList.Add("-o");
            process.StartInfo.ArgumentList.Add(signatureFile);
            process.StartInfo.ArgumentList.Add(filePath);

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"✅ File signed successfully: {signatureFile}");
                ExportPublicKey(keyID);
                CreateZip(filePath, signatureFile, "public_key.asc");
            }
            else
            {
                Console.WriteLine($"❌ Error signing the file! {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception occurred: {ex.Message}");
        }
    }


    static void ExportPublicKey(string keyID)
    {
        string publicKeyFile = "public_key.asc";
        
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = "gpg";
            process.StartInfo.ArgumentList.Add("--export");
            process.StartInfo.ArgumentList.Add("--armor");
            process.StartInfo.ArgumentList.Add(keyID);
            
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            using (StreamWriter writer = new StreamWriter(publicKeyFile))
            {
                process.Start();
                writer.Write(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            }

            Console.WriteLine($"✅ Public key exported: {publicKeyFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error exporting public key: {ex.Message}");
        }
    }

    static void CreateZip(string filePath, string signatureFile, string publicKeyFile)
    {
        string zipFileName = "output.zip";

        try
        {
            using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                archive.CreateEntryFromFile(signatureFile, Path.GetFileName(signatureFile));
                archive.CreateEntryFromFile(publicKeyFile, Path.GetFileName(publicKeyFile));
            }

            Console.WriteLine($"✅ ZIP file created: {zipFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating ZIP file: {ex.Message}");
        }
    }

    static void Main()
    {
        Console.Write("Enter file path to sign: ");
        string? filePath = "ModbusTCPMaster.zip";

        Console.Write("Enter GPG Key ID: ");
        string? keyID = "576058DD4A6CC331";

        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(keyID))
        {
            Console.WriteLine("❌ Error: File path or Key ID cannot be empty.");
            return;
        }

        SignFile(filePath, keyID);
    }
}