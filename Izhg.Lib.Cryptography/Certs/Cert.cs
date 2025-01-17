﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IziHardGames.Tls;

namespace IziHardGames.Libs.Cryptography.Certificates
{
    public static class Cert
    {
        public static List<string> GetDomainsAsFilenames(X509Certificate2 cert)
        {
            List<string> domains = new List<string>();
            var mainDomain = GetDomainForFilename(cert);
            domains.Add(mainDomain);

            if (cert.Extensions.Count > 0)
            {
                foreach (X509Extension extension in cert.Extensions)
                {
                    AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);

                    if (extension.Oid.FriendlyName == "Subject Alternative Name")
                    {
                        string[] asn = asndata.Format(true).Split("\r\n");

                        for (int i = 0; i < asn.Length; i++)
                        {
                            var val = asn[i];
                            if (string.IsNullOrEmpty(val)) continue;

                            string domain = GetDomainForFilename(val.Split("=")[1]);

                            if (!domains.Contains(domain))
                            {
                                domains.Add(domain);
                            }
                        }
                    }
                    //Console.WriteLine("Extension type: {0}", extension.Oid.FriendlyName);
                    //Console.WriteLine("Oid value: {0}", extension.Oid.Value);
                    //Console.WriteLine("Raw data length: {0} {1}", asndata.RawData.Length, Environment.NewLine);
                }
            }
            return domains;
        }
        private static List<string> GetSujectAlternativeName(X509Certificate2 cert)
        {
            var result = new List<string>();
            var subjectAlternativeName = cert.Extensions.Cast<X509Extension>()
                .Where(n => n.Oid.Value == "2.5.29.17")
                .Select(n => new AsnEncodedData(n.Oid, n.RawData))
                .Select(n => n.Format(true))
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(subjectAlternativeName))
            {
                result.AddRange(subjectAlternativeName.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
            }
            return result;
        }
        public static string GetDomainForFilename(string host)
        {
            return host.Replace('*', '_');
        }
        public static string GetDomainForFilename(X509Certificate2 cert)
        {
            string subjectName = cert.SubjectName.Name;
            string[] subjectParts = subjectName.Split(',');
            string domain = GetDomainForFilename(subjectParts.FirstOrDefault(p => p.StartsWith("CN=")).Substring(3));
            return domain;
        }

        public static string CertToKey(X509Certificate2 origin)
        {
            return origin.SerialNumber;
        }
        public static string CertToKey(X509Certificate2 forged, X509Certificate2 origin)
        {
            return origin.SerialNumber;
        }

        internal static string CertToFilename(X509Certificate2 origin)
        {
            return origin.SerialNumber;
        }
        internal static string CertToFilename(X509Certificate2 forged, X509Certificate2 origin)
        {
            return CertToKey(forged, origin);
        }

        public static bool CompareDates(X509Certificate2 left, X509Certificate2 right)
        {
            throw new NotImplementedException();
        }
        public static bool CompareCerts(X509Certificate2 left, X509Certificate2 right)
        {
            if (left.Thumbprint == right.Thumbprint) return true;
            if (left.SerialNumber == right.SerialNumber) return true;
            if (left == right) return true;
            if (GetIssueDate(left) != GetIssueDate(right)) return false;
            if (GetDateExpiration(left) != GetDateExpiration(right)) return false;
            return true;
        }

        public static DateTime GetDateExpiration(X509Certificate2 cert)
        {
            throw new NotImplementedException();
        }
        public static DateTime GetIssueDate(X509Certificate2 cert)
        {
            throw new NotImplementedException();
        }
        //public static string DomainToKey(string domain)
        //{
        //    return GetDomainForFilename(domain);
        //}

        public static X509Certificate2 FromFile(string pathCert, string pathKey)
        {
            return CertManager.LoadPemFromFile(pathCert, pathKey);
        }
    }
}
