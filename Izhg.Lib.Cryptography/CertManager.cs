using IziHardGames.Libs.Cryptography.Certificates;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dic = System.Collections.Concurrent.ConcurrentDictionary<string, System.Security.Cryptography.X509Certificates.X509Certificate2>;

namespace IziHardGames.Tls
{
    public class CertManager
    {

        public static CertManager Shared;
        public readonly Dic cacheForged = new Dic();
        public readonly Dic cacheOriginal = new Dic();
        private string PATH_TO_CERT_CACHE_FORGED = @"C:\Users\ngoc\Documents\Builds\cert cache forged";
        private string PATH_TO_CERT_CACHE_ORIGINAL = @"C:\Users\ngoc\Documents\Builds\cert cache original";

        public CertManager(string pathForged, string pathOriginal)
        {
            this.PATH_TO_CERT_CACHE_FORGED = pathForged;
            this.PATH_TO_CERT_CACHE_ORIGINAL = pathOriginal;

            LoadCache();
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

        public static void CreateDefault(string pathForged, string pathOriginal)
        {
            Shared = new CertManager(pathForged, pathOriginal);
        }

        public static X509Certificate2 GenerateCertCA(X509Certificate cert, string subject = "CN=myauthority.ru")
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

        public static X509Certificate2 LoadPemFromFile(string certPath, string keyPath)
        {
            try
            {
                X509Certificate2 cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
                return cert;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        public void AddCertIntoStore(X509Certificate2 cert)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
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

        public void ClearCache()
        {
            throw new System.NotImplementedException();
        }

        public X509Certificate2 FindCertInStore(StoreName name, StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
            var cert = certs[0];
            store.Close();
            return cert;
        }

        public X509Certificate2 FindCertInStore()
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

        public async ValueTask<X509Certificate2> ForgedGetOrCreateCertFromCacheAsync(X509Certificate2 original, X509Certificate2 ca)
        {
            if (TryFindAppropriateCert(original, cacheForged, out var existed))
            {
                if (Check(original, existed))
                {
                    return existed;
                }
                else
                {
                    return ForgedUpdateCache(original, ca);
                }
            }
            else
            {
                return await ForgedCreateInCache(original, ca).ConfigureAwait(false);
            }
        }

        public void OriginalSaveToCacheSingle(string key, X509Certificate2 cert)
        {
            if (cacheOriginal.TryGetValue(key, out var existed))
            {
                if (!Cert.CompareCerts(existed, cert))
                {
                    var ex = new ArgumentException($"Certs have same key but comparison is Failed");
                    Logger.LogException(ex);
                    throw ex;
                }
            }
            else
            {
                if (!cacheOriginal.TryAdd(key, cert))
                {
                    throw new ArgumentException($"Key [{key}] is Already Exist");
                }
            }
        }

        public async Task OriginalSaveToCacheAndFileWithMultipleDomainsAsync(X509Certificate2 cert)
        {
            var domains = Cert.GetDomainsAsFilenames(cert);

            foreach (var domain in domains)
            {
                var key = Cert.DomainToKey(domain);
                OriginalSaveToCacheSingle(key, cert);
            }
            await SaveOrOverrideCertToFile(PATH_TO_CERT_CACHE_ORIGINAL, cert).ConfigureAwait(false);
        }

        public bool OriginalTryGetCertFromCache(string hostAddress, out X509Certificate2 result)
        {
            if (cacheOriginal.TryGetValue(hostAddress, out result))
            {
                return true;
            }
            return false;
        }

        public bool OriginalTryGetCertFromCacheWithWildcardSearching(string hostAddress, out X509Certificate2 result)
        {
            if (OriginalTryGetCertFromCache(hostAddress, out result))
            {
                return true;
            }

            if (OriginalTryFindCertWithWildcard(hostAddress, cacheOriginal, out result))
            {
                return true;
            }
            result = default;
            return false;
        }

        public bool TryFindCertInStore(out X509Certificate2 cert)
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

        public bool TryFindAppropriateCert(X509Certificate2 cert, Dic container, out X509Certificate2? result)
        {
            string key = Cert.CertToKey(cert);

            if (container.TryGetValue(key, out X509Certificate2? existed))
            {
                result = existed;
                return true;
            }
            else
            {
                var domains = Cert.GetDomainsAsFilenames(cert);

                foreach (var domain in domains)
                {
                    key = Cert.DomainToKey(domain);
                    if (container.TryGetValue(key, out existed))
                    {
                        result = existed;
                        return true;
                    }
                }
                result = null;
                return false;
            }
        }
        public X509Certificate2 FindAppropriateCert(X509Certificate2 cert, Dic container)
        {
            throw new System.NotImplementedException();
        }

        private static bool Check(X509Certificate2 original, X509Certificate2 fake)
        {
            // check expiration time
            // Not Implemented
            return true;
        }

        private static void EnsureDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// Note: host iteself is not included
        /// </summary>
        /// <param name="host"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static bool OriginalTryFindCertWithWildcard(string host, Dic cacheOriginal, out X509Certificate2 result)
        {
            int domainLevels = host.Count(x => x == '.');
            var currentHost = host;

            if (domainLevels > 1)
            {
                for (int i = 0; i < domainLevels; i++)
                {
                    currentHost = ReplaceSubdomainWithWildcard(currentHost);

                    if (cacheOriginal.TryGetValue(currentHost, out result))
                    {
                        return true;
                    }
                }
            }
            result = default;
            return false;
        }

        private static string ReplaceSubdomainWithWildcard(string address)
        {
            return $"_{address.Substring(address.IndexOf('.'))}";
        }

        private static async Task SaveOrOverrideCertToFile(string dir, X509Certificate2 cert)
        {
            string fileName = Cert.CertToFilename(cert);

            string fullPath = Path.Combine(dir, fileName);

            EnsureDirectory(dir);

            if (File.Exists(fullPath))
            {
                await File.WriteAllBytesAsync(fullPath, cert.RawData).ConfigureAwait(false);
                return;
            }
            await File.WriteAllBytesAsync(fullPath, cert.Export(X509ContentType.SerializedCert)).ConfigureAwait(false);
        }

        // https://www.codeproject.com/Articles/5315010/How-to-Use-Certificates-in-ASP-NET-Core
        private static X509Certificate2 TestGenerate(X509Certificate cert)
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
        private static X509Certificate2 ForgedUpdateCache(X509Certificate2 original, X509Certificate2 ca)
        {
            throw new NotImplementedException();
        }


        private async ValueTask<X509Certificate2> ForgedCreateInCache(X509Certificate2 original, X509Certificate2 ca)
        {
            var cert = GenerateCertEndpoint(original, ca, original.NotAfter);
            await ForgedSaveToCacheWithMultipleDomainAsync(cert).ConfigureAwait(false);
            return cert;
        }

        private void ForgedSaveToCache(string key, X509Certificate2 cert)
        {
            try
            {
                if (!cacheForged.TryAdd(key, cert))
                {
                    throw new ArgumentException($"Key [{key}] is Already Exist");
                }
            }
            catch (Exception ex)
            {
                var existed = cacheForged[key];
                Cert.CompareCerts(cert, existed);
                throw ex;
            }
        }

        private async Task ForgedSaveToCacheWithMultipleDomainAsync(X509Certificate2 cert)
        {   // может быть ситуация с перекрестными доменами. 2 сертификата (wildcard и без) будут иметь 2 домена
            var domains = Cert.GetDomainsAsFilenames(cert);

            foreach (var domain in domains)
            {
                var key = Cert.DomainToKey(domain);
                if (!cacheForged.ContainsKey(key))
                {
                    ForgedSaveToCache(key, cert);
                }
            }
            await SaveOrOverrideCertToFile(PATH_TO_CERT_CACHE_FORGED, cert);
        }

        private void LoadCache()
        {
            EnsureDirectory(PATH_TO_CERT_CACHE_FORGED);
            EnsureDirectory(PATH_TO_CERT_CACHE_ORIGINAL);

            var fileNamesForged = Directory.GetFiles(PATH_TO_CERT_CACHE_FORGED);

            for (int i = 0; i < fileNamesForged.Length; i++)
            {
                var cert = LoadCertIntoCache(PATH_TO_CERT_CACHE_FORGED, fileNamesForged[i], cacheForged);
            }

            var fileNamesOriginal = Directory.GetFiles(PATH_TO_CERT_CACHE_ORIGINAL);

            for (int i = 0; i < fileNamesOriginal.Length; i++)
            {
                var cert = LoadCertIntoCache(PATH_TO_CERT_CACHE_ORIGINAL, fileNamesOriginal[i], cacheOriginal);
            }
        }

        private X509Certificate2 LoadCertIntoCache(string dir, string fileName, Dic cache)
        {
            string path = Path.Combine(dir, fileName);
            X509Certificate2 cert = new X509Certificate2(path);
            var domains = Cert.GetDomainsAsFilenames(cert);
            // possible overlaps?
            foreach (var item in domains)
            {
                var key = Cert.DomainToKey(item);
                if (!cache.TryAdd(key, cert))
                {
                    throw new ArgumentException($"Key [{key}] Is Already exist");
                }
            }
            return cert;
        }

        public async Task<bool> OriginTryUpdateAsync(X509Certificate2 certOrigin)
        {
            if (TryFindAppropriateCert(certOrigin, cacheOriginal, out var existed))
            {
                if (Cert.CompareCerts(certOrigin, existed))
                {
                    return false;
                }
                else
                {
                    await OriginUpdateAsync(certOrigin, existed).ConfigureAwait(false);
                    return true;
                }
            }
            else
            {
                await OriginalSaveToCacheAndFileWithMultipleDomainsAsync(certOrigin); return true;
            }
        }
        public async Task OriginUpdateAsync(X509Certificate2 cert, X509Certificate2 existed)
        {
            await UpdateAsync(cert, existed, cacheOriginal, PATH_TO_CERT_CACHE_ORIGINAL).ConfigureAwait(false);
        }
        public async Task UpdateAsync(X509Certificate2 newCert, X509Certificate2 oldCert, Dic cache, string dir)
        {
            var domains = Cert.GetDomainsAsFilenames(newCert);

            foreach (var domain in domains)
            {
                var key = Cert.DomainToKey(domain);
                if (!cache.TryUpdate(key, newCert, oldCert))
                {
                    throw new ArgumentException($"Key [{key}] is not presented");
                }
            }
            await SaveOrOverrideCertToFile(dir, newCert).ConfigureAwait(false);
        }
    }
}