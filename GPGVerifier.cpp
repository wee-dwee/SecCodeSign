#include <iostream>
#include <cstdlib>
#include <vector>
#include <cstdio>
#include <dirent.h>

using namespace std;

void extractZip(const string &zipFile)
{
    cout << "Extracting " << zipFile << "...\n";
    string command = "unzip -o " + zipFile;
    int result = system(command.c_str());

    if (result == 0)
    {
        cout << "✅ Extraction complete.\n";
    }
    else
    {
        cerr << "❌ Failed to extract ZIP file.\n";
        exit(1);
    }
}

void importPublicKey(const string &publicKeyFile)
{
    cout << "Importing public key...\n";
    string command = "gpg --import " + publicKeyFile;
    int result = system(command.c_str());

    if (result == 0)
    {
        cout << "✅ Public key imported successfully.\n";
    }
    else
    {
        cerr << "❌ Failed to import public key.\n";
        exit(1);
    }
}

void verifySignature(const string &filePath)
{
    string signatureFile = filePath + ".sig";
    string command = "gpg --verify " + signatureFile + " " + filePath;
    cout<<signatureFile<<" "<<filePath<<endl;
    cout << "Verifying signature...\n";
    int result = system(command.c_str());

    if (result == 0)
    {
        cout << "✅ Signature verified successfully. The file is authentic.\n";
    }
    else
    {
        cerr << "❌ Signature verification failed! Possible tampering detected.\n";
        exit(2);
    }
}

// Function to list files in the current directory (C++11 alternative to std::filesystem)
vector<string> listFiles()
{
    vector<string> files;
    DIR *dir;
    struct dirent *ent;

    if ((dir = opendir(".")) != NULL)
    {
        while ((ent = readdir(dir)) != NULL)
        {
            string fileName = ent->d_name;
            if (fileName != "." && fileName != "..")
            {
                files.push_back(fileName);
            }
        }
        closedir(dir);
    }
    else
    {
        cerr << "❌ Error: Cannot open directory.\n";
        exit(1);
    }

    return files;
}

int main()
{
    string zipFile = "output.zip";

    // Check if output.zip exists
    if (system(("test -f " + zipFile).c_str()) != 0)
    {
        cerr << "❌ Error: " << zipFile << " not found!\n";
        return 1;
    }

    // Extract ZIP file
    extractZip(zipFile);

    // Identify extracted files
    string publicKeyFile = "public_key.asc";
    string signatureFile="ModbusTCPMaster.zip.sig";
    string originalFile="ModbusTCPMaster.zip";

    vector<string> files = listFiles();
    for (const string &file : files)
    {
        if (file.size() >= 4 && file.substr(file.size() - 4) == ".sig")
        {
            signatureFile = file;
            originalFile = file.substr(0, file.size() - 4); // Remove ".sig" to get original file name
        }
    }

    if (signatureFile.empty() || originalFile.empty())
    {
        cerr << "❌ Error: Could not locate signature or original file.\n";
        return 1;
    }

    // Import the public key
    importPublicKey(publicKeyFile);

    // Verify the signature
    verifySignature(originalFile);

    return 0;
}