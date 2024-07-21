using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ALT_ERP3.Areas.Accounts.Controllers
{
    public class TestingController : Controller
    {
        // GET: Accounts/Testing

        public ActionResult dgsg( string No)
        {
            string sourceDocument = "C:\\Users\\Shruham\\Desktop\\PrintSingleDocumentCrystal.pdf";
            string destinationPath = sourceDocument.Replace(".pdf", "_signed.pdf");
            //Stream stream = File.OpenRead(HttpContext.Current.Server.MapPath($"~/bin/Signatures/fileName"));
            FileStream fileStream = new FileStream("C:\\CERT0-2047-SHA1withRSA.pfx", FileMode.Open);
            SignPdfFile(sourceDocument, destinationPath, fileStream, "123456789", "reason", "location");

            return null;

        }
        public void SignPdfFile(string sourceDocument, string destinationPath, Stream privateKeyStream, string password, string reason, string location)
        {
            Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, password.ToCharArray());
            privateKeyStream.Dispose();
            string alias = null;
            foreach (string tAlias in pk12.Aliases)
            {
                if (pk12.IsKeyEntry(tAlias))
                {
                    alias = tAlias;
                    break;
                }
            }
            var pk = pk12.GetKey(alias).Key;
            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(sourceDocument);
            using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (iTextSharp.text.pdf.PdfStamper stamper = iTextSharp.text.pdf.PdfStamper.CreateSignature(reader, fout, '\0'))
                {
                    iTextSharp.text.pdf.PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                    iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont(System.Web.HttpContext.Current.Server.MapPath("~/Content/fonts/Arial.ttf"), iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
                    iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 11);
                    appearance.Layer2Font = font;
                    //appearance.Image = new iTextSharp.text.pdf.PdfImage();
                    appearance.Reason = reason;
                    appearance.Location = location;
                    appearance.SetVisibleSignature(new iTextSharp.text.Rectangle(20, 10, 170, 60), 1, "Icsi-Vendor");
                    iTextSharp.text.pdf.security.IExternalSignature es = new iTextSharp.text.pdf.security.PrivateKeySignature(pk, "SHA-256");
                    iTextSharp.text.pdf.security.MakeSignature.SignDetached(appearance, es, new X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, iTextSharp.text.pdf.security.CryptoStandard.CMS);
                    stamper.Close();
                }
            }
        }


        /// <summary>
        /// Signs a PDF document using iTextSharp library
        /// </summary>
        /// <param name=”sourceDocument”>The path of the source pdf document which is to be signed</param>
        /// <param name=”destinationPath”>The path at which the signed pdf document should be generated</param>
        /// <param name=”privateKeyStream”>A Stream containing the private/public key in .pfx format which would be used to sign the document</param>
        /// <param name=”keyPassword”>The password for the private key</param>
        /// <param name=”reason”>String describing the reason for signing, would be embedded as part of the signature</param>
        /// <param name=”location”>Location where the document was signed, would be embedded as part of the signature</param>
        public static void signPdfFile(string sourceDocument, string destinationPath, Stream privateKeyStream, string keyPassword, string reason, string location)
        {
            FileStream fileStream = new FileStream("C:\\CERT0-2047-SHA1withRSA.pfx", FileMode.Open);

            Pkcs12Store pk12 = new Pkcs12Store(privateKeyStream, keyPassword.ToCharArray());
            privateKeyStream.Dispose();

            //then Iterate throught certificate entries to find the private key entry
            string alias = null;
            foreach (string tAlias in pk12.Aliases)
            {
                if (pk12.IsKeyEntry(tAlias))
                {
                    alias = tAlias;
                    break;
                }
            }
            var pk = pk12.GetKey(alias).Key;

            // reader and stamper
            PdfReader reader = new PdfReader(sourceDocument);
            using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (PdfStamper stamper = PdfStamper.CreateSignature(reader, fout, '\0'))
                {
                    // appearance
                    PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                    //appearance.Image = new iTextSharp.text.pdf.PdfImage();
                    appearance.Reason = reason;
                    appearance.Location = location;
                    appearance.SetVisibleSignature(new iTextSharp.text.Rectangle(20, 10, 170, 60), 1, "Icsi -Vendor");
                    // digital signature
                    IExternalSignature es = new PrivateKeySignature(pk, "SHA-256");
                    MakeSignature.SignDetached(appearance, es, new X509Certificate[] { pk12.GetCertificate(alias).Certificate }, null, null, null, 0, CryptoStandard.CMS);

                    stamper.Close();
                }
            }
        }
    }



    /// <summary>
    /// Verifies the signature of a prevously signed PDF document using the specified public key
    /// </summary>
    /// <param name=”pdfFile”>a Previously signed pdf document</param>
    /// <param name=”publicKeyStream”>Public key to be used to verify the signature in .cer format</param>
    /// <exception cref=”System.InvalidOperationException”>Throw System.InvalidOperationException if the document is not signed or the signature could not be verified</exception>
    //public void verifyPdfSignature(string pdfFile, Stream publicKeyStream)
    //{
    //    var parser = new X509CertificateParser();
    //    var certificate = parser.ReadCertificate(publicKeyStream);
    //    publicKeyStream.Dispose();

    //    PdfReader reader = new PdfReader(pdfFile);
    //    AcroFields af = reader.AcroFields;
    //    var names = af.GetSignatureNames();

    //    if (names.Count == 0)
    //    {
    //        throw new InvalidOperationException("No Signature present in pdf file.");
    //    }

    //    foreach (string name in names)
    //    {
    //        if (!af.SignatureCoversWholeDocument(name))
    //        {
    //            throw new InvalidOperationException(string.Format("The signature: { 0 } does not covers the whole document.", name));
    //        }

    //        PdfPKCS7 pk = af.VerifySignature(name);
    //        var cal = pk.SignDate;
    //        var pkc = pk.Certificates;

    //        if (!pk.Verify())
    //        {
    //            throw new InvalidOperationException("The signature could not be verified.");
    //        }
    //        if (!pk.VerifyTimestampImprint())
    //        {
    //            throw new InvalidOperationException("The signature timestamp could not be verified.");
    //        }

    //        Object[] fails = CertificateVerification.VerifyCertificates(pkc, new X509Certificate[] { certificate }, null, cal);
    //        if (fails != null)
    //        {
    //            throw new InvalidOperationException("The file is not signed using the specified key - pair.");
    //        }
    //    }
    //}

}
