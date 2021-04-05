# https://stackoverflow.com/questions/58136779/how-to-create-a-signing-certificate-and-use-it-in-identityserver4-in-production

function New-SigningCertificate
{
	param
	(
		[Parameter(Mandatory)]
		[int]$index,
		[bool]$privateKey = $false
	)

	$certificateStoreLocation = "CERT:\CurrentUser\My\";
	$name = "Signing-Certificate-$($index)";
	$notAfter = [DateTime]::MaxValue;
	$notBefore = [DateTime]::MinValue.AddYears(1899);

	$certificate = New-SelfsignedCertificate `
		-CertStoreLocation $certificateStoreLocation `
		-HashAlgorithm SHA256 `
		-KeyAlgorithm RSA `
		-KeyExportPolicy Exportable `
		-KeyLength 2048 `
		-KeySpec Signature `
		-NotAfter $notAfter `
		-NotBefore $notBefore `
		-Subject "CN=$($name)";

	if($privateKey)
	{
		$password = ConvertTo-SecureString -AsPlainText -Force -String "password";
		Export-PfxCertificate -Cert $certificate -FilePath "$($PSScriptRoot)\$($name).pfx" -Password $password;
	}
	else
	{
		Export-Certificate -Cert $certificate -FilePath "$($PSScriptRoot)\$($name).cer";
	}

	Remove-Item -Path "$($certificateStoreLocation)$($certificate.Thumbprint)";
}

New-SigningCertificate `
	-Index 1 `
	-PrivateKey $true;

New-SigningCertificate `
	-Index 2;

New-SigningCertificate `
	-Index 3;

Write-Host;
Write-Host "Signing-certificates created. Press any key to exit.";
Read-Host;