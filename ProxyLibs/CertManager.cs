﻿// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IziHardGames.Tls
{
    public class CertManager
    {
        public X509Certificate2 FindCert(StoreName name, StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
            var cert = certs[0];
            store.Close();
            return cert;
        }
        public X509Certificate2 FindCert()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var arr = store.Certificates.ToArray();
            //var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "www.google.com", false);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "IziHardGames_MITM", false);
            var cert = certs[0];
            store.Close();
            return cert;
        }
        // https://www.codeproject.com/Articles/5315010/How-to-Use-Certificates-in-ASP-NET-Core
        public X509Certificate2 Generate(X509Certificate cert)
        {
            // Generate private-public key pair
            var rsaKey = RSA.Create(2048);

            // Describe certificate
            string subjectName = cert.Subject;

            // Create certificate request
            var certificateRequest = new CertificateRequest(
                subjectName,
                rsaKey,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            certificateRequest.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(
                    certificateAuthority: false,
                    hasPathLengthConstraint: false,
                    pathLengthConstraint: 0,
                    critical: true
                )
            );

            certificateRequest.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    keyUsages:
                        X509KeyUsageFlags.DigitalSignature
                        | X509KeyUsageFlags.KeyEncipherment,
                    critical: false
                )
            );

            certificateRequest.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(
                    key: certificateRequest.PublicKey,
                    critical: false
                )
            );

            var expireAt = DateTimeOffset.Now.AddYears(5);

            var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.Now, expireAt);

            // Export certificate with private key
            var exportableCertificate = new X509Certificate2(certificate.Export(X509ContentType.Cert),
                                                             (string)null,
                                                             X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(rsaKey);

            exportableCertificate.FriendlyName = "Ivan Yakimov Test-only Certificate For Client Authorization";

            return exportableCertificate;
        }

        public X509Certificate2 GenerateCertCA(X509Certificate cert, string subject = "CN=myauthority.ru")
        {
            var rsaKey = RSA.Create(2048);
            var certReq = new CertificateRequest(subject, rsaKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, false));

            var expirate = DateTimeOffset.Now.AddYears(5);
            X509Certificate2 caCert = certReq.CreateSelfSigned(DateTimeOffset.Now, expirate);
            return caCert;
        }

        public static X509Certificate2 GenerateCertEndpoint(X509Certificate2 donor, X509Certificate2 caCert, DateTimeOffset expirate)
        {
            string subject = donor.Subject;

            var clientKey = RSA.Create(2048);
            var clientReq = new CertificateRequest(subject, clientKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            foreach (var extension in donor.Extensions)
            {
                clientReq.CertificateExtensions.Add(extension);
            }

            byte[] serialNumber = BitConverter.GetBytes(DateTime.Now.ToBinary());
            var clientCert = clientReq.Create(caCert, DateTimeOffset.Now, expirate, serialNumber);
            var certWithKey = clientCert.CopyWithPrivateKey(clientKey);
            return new X509Certificate2(certWithKey.Export(X509ContentType.Pkcs12));
        }

        public void CertToFile(X509Certificate2 clientCert, RSA clientKey)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(clientCert.RawData, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
            File.WriteAllText("public.crt", builder.ToString());


            var exportCert = new X509Certificate2(clientCert.Export(X509ContentType.Cert), (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(clientKey);
            File.WriteAllBytes("client.pfx", exportCert.Export(X509ContentType.Pfx));
            File.WriteAllBytes("client.p12", exportCert.Export(X509ContentType.Pkcs12));
        }

        public void Store(X509Certificate2 cert)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
        }


        public bool TryFindCert(out X509Certificate2 cert)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);


            X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, "CN=www.google.com", false);

            if (cers.Count > 0)
            {
                var cer = cers[0];
            };
            store.Close();

            foreach (X509Certificate2 certificate in store.Certificates)
            {
                //TODO's
            }
            throw new System.NotImplementedException();
        }

        //https://gist.github.com/sinmygit/4c819c34b1c450fd23574970b6650a78
        public static X509Certificate2 CreateCertificate(string certSubject, bool isCA)
        {
            //string CAsubject = certSubject;
            //CX500DistinguishedName dn = new CX500DistinguishedName();

            //dn.Encode("CN=" + CAsubject, X500NameFlags.XCN_CERT_NAME_STR_NONE);

            //string strRfc822Name = certSubject;

            //CAlternativeName objRfc822Name = new CAlternativeName();
            //CAlternativeNames objAlternativeNames = new CAlternativeNames();
            //CX509ExtensionAlternativeNames objExtensionAlternativeNames = new CX509ExtensionAlternativeNames();

            //// Set Alternative RFC822 Name 
            //objRfc822Name.InitializeFromString(AlternativeNameType.XCN_CERT_ALT_NAME_DNS_NAME, strRfc822Name);

            //// Set Alternative Names 
            //objAlternativeNames.Add(objRfc822Name);
            //objExtensionAlternativeNames.InitializeEncode(objAlternativeNames);
            ////objPkcs10.X509Extensions.Add((CX509Extension)objExtensionAlternativeNames);





            //DistinguishedName

            ////Issuer Property for cleanup
            //string issuer = "__Interceptor_Trusted_Root";
            //CX500DistinguishedName issuerdn = new CX500DistinguishedName();

            //issuerdn.Encode("CN=" + issuer, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            //// Create a new Private Key

            //CX509PrivateKey key = new CX509PrivateKey();
            //key.ProviderName = "Microsoft Enhanced RSA and AES Cryptographic Provider"; //"Microsoft Enhanced Cryptographic Provider v1.0"
            //                                                                            // Set CAcert to 1 to be used for Signature
            //if (isCA)
            //{
            //    key.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
            //}
            //else
            //{
            //    key.KeySpec = X509KeySpec.XCN_AT_KEYEXCHANGE;
            //}
            //key.Length = 2048;
            //key.MachineContext = true;
            //key.Create();

            //// Create Attributes
            ////var serverauthoid = new X509Enrollment.CObjectId();
            //CObjectId serverauthoid = new CObjectId();
            //serverauthoid.InitializeFromValue("1.3.6.1.5.5.7.3.1");
            //CObjectIds ekuoids = new CObjectIds();
            //ekuoids.Add(serverauthoid);
            //CX509ExtensionEnhancedKeyUsage ekuext = new CX509ExtensionEnhancedKeyUsage();
            //ekuext.InitializeEncode(ekuoids);

            //CX509CertificateRequestCertificate cert = new CX509CertificateRequestCertificate();
            //cert.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextMachine, key, "");
            //cert.Subject = dn;
            //cert.Issuer = issuerdn;
            //cert.NotBefore = (DateTime.Now).AddDays(-1);//Backup One day to Avoid Timing Issues
            //cert.NotAfter = cert.NotBefore.AddDays(90); //Arbitrary... Change to persist longer...
            //                                            //Use Sha256
            //CObjectId hashAlgorithmObject = new CObjectId();
            //hashAlgorithmObject.InitializeFromAlgorithmName(ObjectIdGroupId.XCN_CRYPT_HASH_ALG_OID_GROUP_ID, 0, 0, "SHA256");
            //cert.HashAlgorithm = hashAlgorithmObject;

            //cert.X509Extensions.Add((CX509Extension)ekuext);
            //cert.X509Extensions.Add((CX509Extension)objExtensionAlternativeNames);
            ////https://blogs.msdn.microsoft.com/alejacma/2011/11/07/how-to-add-subject-alternative-name-to-your-certificate-requests-c/
            //if (isCA)
            //{
            //    CX509ExtensionBasicConstraints basicConst = new CX509ExtensionBasicConstraints();
            //    basicConst.InitializeEncode(true, 1);
            //    cert.X509Extensions.Add((CX509Extension)basicConst);
            //}
            //else
            //{
            //    var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //    store.Open(OpenFlags.ReadOnly);
            //    X509Certificate2Collection signer = store.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root", false);

            //    CSignerCertificate signerCertificate = new CSignerCertificate();
            //    signerCertificate.Initialize(true, 0, EncodingType.XCN_CRYPT_STRING_HEX, signer[0].Thumbprint);
            //    cert.SignerCertificate = signerCertificate;
            //}
            //cert.Encode();

            //CX509Enrollment enrollment = new CX509Enrollment();
            //enrollment.InitializeFromRequest(cert);
            //string certdata = enrollment.CreateRequest(0);
            //enrollment.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedCertificate, certdata, 0, "");

            //if (isCA)
            //{

            //    //Install CA Root Certificate
            //    X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //    store.Open(OpenFlags.ReadOnly);
            //    X509Certificate2Collection certList = store.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root", false);
            //    store.Close();

            //    X509Store rootStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            //    rootStore.Open(OpenFlags.ReadWrite);
            //    X509Certificate2Collection rootcertList = rootStore.Certificates.Find(X509FindType.FindBySubjectName, "__Interceptor_Trusted_Root", false);
            //    rootStore.Add(certList[0]);
            //    rootStore.Close();
            //    return certList[0];
            //}
            //else
            //{
            //    //Return Per Domain Cert
            //    X509Store xstore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //    xstore.Open(OpenFlags.ReadOnly);
            //    X509Certificate2Collection certList = xstore.Certificates.Find(X509FindType.FindBySubjectName, certSubject, false);
            //    xstore.Close();
            //    return certList[0];
            //}
            throw new System.NotImplementedException();
        }

        public static X509Certificate2 LoadPemFromFile(string certPath, string keyPath)
        {
            X509Certificate2 cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            return cert;
        }
    }
}