# Azure Services Certificates example

An example that uses client certificates to access AZure Key Vaults service from a non-Azure deployed web application.

The applicatin takes care of installing the certificate on the client machine by passing the certificate as a Base64 string in the startup parameters.

The application is made to investigate the problem on [StackOverflow][stackoverflow]

## Get Started

Create a Certificate and configure your Azure Key Vault as described [here][azureconfig]. 

```powershell
# create .pfx certificate
PS C:\Windows\system32> $cert = New-SelfSignedCertificate -certstorelocation cert:\localmachine\my -dnsname "dev.skan.ch"
PS C:\Windows\system32> $pwd = ConvertTo-SecureString -String 'passw0rd!' -Force -AsPlaintext
PS C:\Windows\system32> $path = 'cert:\localmachine\my\'+$cert.thumbprint
PS C:\Windows\system32> Export-PfxCertificate -cert $path -FilePath c:\temp\cert.pfx -Password $pwd

    Directory: C:\temp

Mode                LastWriteTime         Length Name
----                -------------         ------ ----
-a----         9/6/2019   3:19 PM           2653 cert.pfx

# create .cer certificate for upload to Azure
PS C:\Windows\system32> Get-PfxCertificate -FilePath c:\temp\cert.pfx | Export-Certificate -FilePath c:\temp\cert.cer -Type CERT
Enter password: *********

    Directory: C:\temp

Mode                LastWriteTime         Length Name
----                -------------         ------ ----
-a----         9/6/2019   3:24 PM            802 cert.cer

# Get the Base64 string of the .pfx
$bytes = Get-Content 'C:\temp\cert.pfx' -Encoding Byte
$base64string = [System.Convert]::ToBase64String($bytes)
```

## Run the example

Modify the appsettings.json with your azure information.

```json
  "Vault": "https://{your-key-vault-name}.vault.azure.net/",
  "ClientId": "{your-client-id}"
```

Run the application with the following parameters

```powershell
dotnet build -c Debug
dotnet .\Certificates\bin\Debug\netcoreapp2.2\Certificates.dll -- -c {certificate-base64-string} -p {certificate-password}
```

Go to `https://localhost:5001/api/values/{my-secret-name}` and retrieve your secret value.

[stackoverflow]: https://stackoverflow.com/questions/57955113/client-certification-exception-when-accessing-azure-key-vaults
[azureconfig]: https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-2.2#use-application-id-and-x509-certificate-for-non-azure-hosted-apps
