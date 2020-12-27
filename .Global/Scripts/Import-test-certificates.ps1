$certificateStoreLocation = "CERT:\CurrentUser\My";

$rootCertificateFileName = "Test-IdentityServer-Root-CA.cer";

$clientCertificateFileName = "Test-IdentityServer-client-certificate.pfx";
$clientCertificateWithEmailFileName = "Test-IdentityServer-client-certificate-with-email.pfx";
$clientCertificateWithUpnFileName = "Test-IdentityServer-client-certificate-with-UPN.pfx";
$clientCertificateWithEmailAndUpnFileName = "Test-IdentityServer-client-certificate-with-email-and-UPN.pfx";
$invalidClientCertificateFileName = "Test-IdentityServer-invalid-client-certificate.pfx";

$scriptDirectoryPath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition;
$globalDirectoryPath = Split-Path -Parent -Path $scriptDirectoryPath;
$certificateDirectoryPath = "$($globalDirectoryPath)\Certificates";

$password = ConvertTo-SecureString -AsPlainText -Force -String "P@ssword12";

Write-Host "You must run this script as an administrator!";

Write-Host;
Write-Host "Press any key to continue...";

Read-Host;

Write-Host;
Write-Host "Importing IdentityServer test-certificates...";

Import-Certificate -CertStoreLocation "CERT:\LocalMachine\Root" -FilePath "$($certificateDirectoryPath)\$($rootCertificateFileName)";

Import-PfxCertificate -CertStoreLocation $certificateStoreLocation -FilePath "$($certificateDirectoryPath)\$($clientCertificateFileName)" -Password $password;
Import-PfxCertificate -CertStoreLocation $certificateStoreLocation -FilePath "$($certificateDirectoryPath)\$($clientCertificateWithEmailFileName)" -Password $password;
Import-PfxCertificate -CertStoreLocation $certificateStoreLocation -FilePath "$($certificateDirectoryPath)\$($clientCertificateWithUpnFileName)" -Password $password;
Import-PfxCertificate -CertStoreLocation $certificateStoreLocation -FilePath "$($certificateDirectoryPath)\$($clientCertificateWithEmailAndUpnFileName)" -Password $password;
Import-PfxCertificate -CertStoreLocation $certificateStoreLocation -FilePath "$($certificateDirectoryPath)\$($invalidClientCertificateFileName)" -Password $password;

Write-Host;
Write-Host "Press any key to close...";

Read-Host;