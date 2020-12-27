$certificateStoreLocation = "CERT:\CurrentUser\My";

$rootCertificateName = "CN=Test-IdentityServer Root CA";

$clientCertificateName = "CN=Test-IdentityServer client-certificate";
$clientCertificateWithEmailName = "CN=Test-IdentityServer client-certificate with email";
$clientCertificateWithUpnName = "CN=Test-IdentityServer client-certificate with UPN";
$clientCertificateWithEmailAndUpnName = "CN=Test-IdentityServer client-certificate with email & UPN";
$invalidClientCertificateName = "CN=Test-IdentityServer invalid client-certificate";

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

$hashAlgorithm = "sha256";
$keyExportPolicy = "Exportable";
$keyLength = 2048;
$keySpec = "Signature";
$notAfter = "9999-12-31";
$type = "Custom";

Write-Host "Creating IdentityServer test-certificates...";

$rootCertificate = New-SelfSignedCertificate `
	-CertStoreLocation $certificateStoreLocation `
	-HashAlgorithm $hashAlgorithm `
	-KeyExportPolicy $keyExportPolicy `
	-KeyLength $keyLength `
	-KeySpec $keySpec `
	-KeyUsage CertSign `
	-KeyUsageProperty Sign `
	-NotAfter $notAfter `
	-Subject $rootCertificateName `
	-Type $type;

$clientCertificate = New-SelfSignedCertificate `
	-CertStoreLocation $certificateStoreLocation `
	-HashAlgorithm $hashAlgorithm `
	-KeyExportPolicy $keyExportPolicy `
	-KeyLength $keyLength `
	-KeySpec $keySpec `
	-NotAfter $notAfter `
	-Signer $rootCertificate `
	-Subject $clientCertificateName `
	-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2") `
	-Type $type;

$clientCertificateWithEmail = New-SelfSignedCertificate `
	-CertStoreLocation $certificateStoreLocation `
	-HashAlgorithm $hashAlgorithm `
	-KeyExportPolicy $keyExportPolicy `
	-KeyLength $keyLength `
	-KeySpec $keySpec `
	-NotAfter $notAfter `
	-Signer $rootCertificate `
	-Subject $clientCertificateWithEmailName `
	-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}email=first-name.last-name@company.com") `
	-Type $type;

$clientCertificateWithUpn = New-SelfSignedCertificate `
	-CertStoreLocation $certificateStoreLocation `
	-HashAlgorithm $hashAlgorithm `
	-KeyExportPolicy $keyExportPolicy `
	-KeyLength $keyLength `
	-KeySpec $keySpec `
	-NotAfter $notAfter `
	-Signer $rootCertificate `
	-Subject $clientCertificateWithUpnName `
	-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}upn=user-name@domain.net") `
	-Type $type;
		
$clientCertificateWithEmailAndUpn = New-SelfSignedCertificate `
	-CertStoreLocation $certificateStoreLocation `
	-HashAlgorithm $hashAlgorithm `
	-KeyExportPolicy $keyExportPolicy `
	-KeyLength $keyLength `
	-KeySpec $keySpec `
	-NotAfter $notAfter `
	-Signer $rootCertificate `
	-Subject $clientCertificateWithEmailAndUpnName `
	-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2","2.5.29.17={text}email=first-name.last-name@company.com&upn=user-name@domain.net") `
	-Type $type;

$invalidClientCertificate = New-SelfSignedCertificate `
	-CertStoreLocation $certificateStoreLocation `
	-HashAlgorithm $hashAlgorithm `
	-KeyExportPolicy $keyExportPolicy `
	-KeyLength $keyLength `
	-KeySpec $keySpec `
	-NotAfter $notAfter `
	-Subject $invalidClientCertificateName `
	-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2") `
	-Type $type;
	
Write-Host;
Write-Host "Exporting IdentityServer test-certificates...";

Export-Certificate -Cert $rootCertificate -FilePath "$($certificateDirectoryPath)\$($rootCertificateFileName)";

Export-PfxCertificate -Cert $clientCertificate -FilePath "$($certificateDirectoryPath)\$($clientCertificateFileName)" -Password $password;
Export-PfxCertificate -Cert $clientCertificateWithEmail -FilePath "$($certificateDirectoryPath)\$($clientCertificateWithEmailFileName)" -Password $password;
Export-PfxCertificate -Cert $clientCertificateWithUpn -FilePath "$($certificateDirectoryPath)\$($clientCertificateWithUpnFileName)" -Password $password;
Export-PfxCertificate -Cert $clientCertificateWithEmailAndUpn -FilePath "$($certificateDirectoryPath)\$($clientCertificateWithEmailAndUpnFileName)" -Password $password;
Export-PfxCertificate -Cert $invalidClientCertificate -FilePath "$($certificateDirectoryPath)\$($invalidClientCertificateFileName)" -Password $password;

Write-Host;
Write-Host "Removing IdentityServer test-certificates from certificate-store $($certificateStoreLocation)...";

Remove-Item -Path "$($certificateStoreLocation)\$($rootCertificate.Thumbprint)";
Remove-Item -Path "$($certificateStoreLocation)\$($clientCertificate.Thumbprint)";
Remove-Item -Path "$($certificateStoreLocation)\$($clientCertificateWithEmail.Thumbprint)";
Remove-Item -Path "$($certificateStoreLocation)\$($clientCertificateWithUpn.Thumbprint)";
Remove-Item -Path "$($certificateStoreLocation)\$($clientCertificateWithEmailAndUpn.Thumbprint)";
Remove-Item -Path "$($certificateStoreLocation)\$($invalidClientCertificate.Thumbprint)";

Write-Host;
Write-Host "Press any key to close...";

Read-Host;