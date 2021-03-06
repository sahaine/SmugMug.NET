﻿namespace SmugMug.NET.Utility
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.IO;
   using System.Linq;
   using System.Net;
   using System.Net.Http;
   using System.Net.Http.Headers;
   using System.Security.Cryptography;
   using System.Threading;
   using System.Threading.Tasks;
   using System.Web;
   using Newtonsoft.Json;
   using SmugMug.v2;
   using SmugMug.v2.Authentication;
   using SmugMug.v2.Types;
   using SmugMugShared.Extensions;

   public class ImageUploader : IUploader
   {
      private readonly OAuthToken _oauthToken;
      private readonly AlbumTemplateEntity _noBuyTemplate;
      private readonly AlbumTemplateEntity _defaultTemplate;
      private readonly NodeEntity _rootNode;

      public NodeEntity CustomersFolder { get; }

      private static readonly Dictionary<string, NodeEntity> _cache = new Dictionary<string, NodeEntity>();

      private static IDictionary<string, string> _mappings = new Dictionary<string, string>() {
         #region Big freaking list of mime types
        // combination of values from Windows 7 Registry and 
        // from C:\Windows\System32\inetsrv\config\applicationHost.config
        // some added, including .7z and .dat
        {".323", "text/h323"},
        {".3g2", "video/3gpp2"},
        {".3gp", "video/3gpp"},
        {".3gp2", "video/3gpp2"},
        {".3gpp", "video/3gpp"},
        {".7z", "application/x-7z-compressed"},
        {".aa", "audio/audible"},
        {".AAC", "audio/aac"},
        {".aaf", "application/octet-stream"},
        {".aax", "audio/vnd.audible.aax"},
        {".ac3", "audio/ac3"},
        {".aca", "application/octet-stream"},
        {".accda", "application/msaccess.addin"},
        {".accdb", "application/msaccess"},
        {".accdc", "application/msaccess.cab"},
        {".accde", "application/msaccess"},
        {".accdr", "application/msaccess.runtime"},
        {".accdt", "application/msaccess"},
        {".accdw", "application/msaccess.webapplication"},
        {".accft", "application/msaccess.ftemplate"},
        {".acx", "application/internet-property-stream"},
        {".AddIn", "text/xml"},
        {".ade", "application/msaccess"},
        {".adobebridge", "application/x-bridge-url"},
        {".adp", "application/msaccess"},
        {".ADT", "audio/vnd.dlna.adts"},
        {".ADTS", "audio/aac"},
        {".afm", "application/octet-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aifc", "audio/aiff"},
        {".aiff", "audio/aiff"},
        {".air", "application/vnd.adobe.air-application-installer-package+zip"},
        {".amc", "application/x-mpeg"},
        {".application", "application/x-ms-application"},
        {".art", "image/x-jg"},
        {".asa", "application/xml"},
        {".asax", "application/xml"},
        {".ascx", "application/xml"},
        {".asd", "application/octet-stream"},
        {".asf", "video/x-ms-asf"},
        {".ashx", "application/xml"},
        {".asi", "application/octet-stream"},
        {".asm", "text/plain"},
        {".asmx", "application/xml"},
        {".aspx", "application/xml"},
        {".asr", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".atom", "application/atom+xml"},
        {".au", "audio/basic"},
        {".avi", "video/x-msvideo"},
        {".axs", "application/olescript"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bin", "application/octet-stream"},
        {".bmp", "image/bmp"},
        {".c", "text/plain"},
        {".cab", "application/octet-stream"},
        {".caf", "audio/x-caf"},
        {".calx", "application/vnd.ms-office.calx"},
        {".cat", "application/vnd.ms-pki.seccat"},
        {".cc", "text/plain"},
        {".cd", "text/plain"},
        {".cdda", "audio/aiff"},
        {".cdf", "application/x-cdf"},
        {".cer", "application/x-x509-ca-cert"},
        {".chm", "application/octet-stream"},
        {".class", "application/x-java-applet"},
        {".clp", "application/x-msclip"},
        {".cmx", "image/x-cmx"},
        {".cnf", "text/plain"},
        {".cod", "image/cis-cod"},
        {".config", "application/xml"},
        {".contact", "text/x-ms-contact"},
        {".coverage", "application/xml"},
        {".cpio", "application/x-cpio"},
        {".cpp", "text/plain"},
        {".crd", "application/x-mscardfile"},
        {".crl", "application/pkix-crl"},
        {".crt", "application/x-x509-ca-cert"},
        {".cs", "text/plain"},
        {".csdproj", "text/plain"},
        {".csh", "application/x-csh"},
        {".csproj", "text/plain"},
        {".css", "text/css"},
        {".csv", "text/csv"},
        {".cur", "application/octet-stream"},
        {".cxx", "text/plain"},
        {".dat", "application/octet-stream"},
        {".datasource", "application/xml"},
        {".dbproj", "text/plain"},
        {".dcr", "application/x-director"},
        {".def", "text/plain"},
        {".deploy", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dgml", "application/xml"},
        {".dib", "image/bmp"},
        {".dif", "video/x-dv"},
        {".dir", "application/x-director"},
        {".disco", "text/xml"},
        {".dll", "application/x-msdownload"},
        {".dll.config", "text/xml"},
        {".dlm", "text/dlm"},
        {".doc", "application/msword"},
        {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".dot", "application/msword"},
        {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
        {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
        {".dsp", "application/octet-stream"},
        {".dsw", "text/plain"},
        {".dtd", "text/xml"},
        {".dtsConfig", "text/xml"},
        {".dv", "video/x-dv"},
        {".dvi", "application/x-dvi"},
        {".dwf", "drawing/x-dwf"},
        {".dwp", "application/octet-stream"},
        {".dxr", "application/x-director"},
        {".eml", "message/rfc822"},
        {".emz", "application/octet-stream"},
        {".eot", "application/octet-stream"},
        {".eps", "application/postscript"},
        {".etl", "application/etl"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".exe", "application/octet-stream"},
        {".exe.config", "text/xml"},
        {".fdf", "application/vnd.fdf"},
        {".fif", "application/fractals"},
        {".filters", "Application/xml"},
        {".fla", "application/octet-stream"},
        {".flr", "x-world/x-vrml"},
        {".flv", "video/x-flv"},
        {".fsscript", "application/fsharp-script"},
        {".fsx", "application/fsharp-script"},
        {".generictest", "application/xml"},
        {".gif", "image/gif"},
        {".group", "text/x-ms-group"},
        {".gsm", "audio/x-gsm"},
        {".gtar", "application/x-gtar"},
        {".gz", "application/x-gzip"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hdml", "text/x-hdml"},
        {".hhc", "application/x-oleobject"},
        {".hhk", "application/octet-stream"},
        {".hhp", "application/octet-stream"},
        {".hlp", "application/winhlp"},
        {".hpp", "text/plain"},
        {".hqx", "application/mac-binhex40"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htt", "text/webviewhtml"},
        {".hxa", "application/xml"},
        {".hxc", "application/xml"},
        {".hxd", "application/octet-stream"},
        {".hxe", "application/xml"},
        {".hxf", "application/xml"},
        {".hxh", "application/octet-stream"},
        {".hxi", "application/octet-stream"},
        {".hxk", "application/xml"},
        {".hxq", "application/octet-stream"},
        {".hxr", "application/octet-stream"},
        {".hxs", "application/octet-stream"},
        {".hxt", "text/html"},
        {".hxv", "application/xml"},
        {".hxw", "application/octet-stream"},
        {".hxx", "text/plain"},
        {".i", "text/plain"},
        {".ico", "image/x-icon"},
        {".ics", "application/octet-stream"},
        {".idl", "text/plain"},
        {".ief", "image/ief"},
        {".iii", "application/x-iphone"},
        {".inc", "text/plain"},
        {".inf", "application/octet-stream"},
        {".inl", "text/plain"},
        {".ins", "application/x-internet-signup"},
        {".ipa", "application/x-itunes-ipa"},
        {".ipg", "application/x-itunes-ipg"},
        {".ipproj", "text/plain"},
        {".ipsw", "application/x-itunes-ipsw"},
        {".iqy", "text/x-ms-iqy"},
        {".isp", "application/x-internet-signup"},
        {".ite", "application/x-itunes-ite"},
        {".itlp", "application/x-itunes-itlp"},
        {".itms", "application/x-itunes-itms"},
        {".itpc", "application/x-itunes-itpc"},
        {".IVF", "video/x-ivf"},
        {".jar", "application/java-archive"},
        {".java", "application/octet-stream"},
        {".jck", "application/liquidmotion"},
        {".jcz", "application/liquidmotion"},
        {".jfif", "image/pjpeg"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpb", "application/octet-stream"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".json", "application/json"},
        {".jsx", "text/jscript"},
        {".jsxbin", "text/plain"},
        {".latex", "application/x-latex"},
        {".library-ms", "application/windows-library+xml"},
        {".lit", "application/x-ms-reader"},
        {".loadtest", "application/xml"},
        {".lpk", "application/octet-stream"},
        {".lsf", "video/x-la-asf"},
        {".lst", "text/plain"},
        {".lsx", "video/x-la-asf"},
        {".lzh", "application/octet-stream"},
        {".m13", "application/x-msmediaview"},
        {".m14", "application/x-msmediaview"},
        {".m1v", "video/mpeg"},
        {".m2t", "video/vnd.dlna.mpeg-tts"},
        {".m2ts", "video/vnd.dlna.mpeg-tts"},
        {".m2v", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".m3u8", "audio/x-mpegurl"},
        {".m4a", "audio/m4a"},
        {".m4b", "audio/m4b"},
        {".m4p", "audio/m4p"},
        {".m4r", "audio/x-m4r"},
        {".m4v", "video/x-m4v"},
        {".mac", "image/x-macpaint"},
        {".mak", "text/plain"},
        {".man", "application/x-troff-man"},
        {".manifest", "application/x-ms-manifest"},
        {".map", "text/plain"},
        {".master", "application/xml"},
        {".mda", "application/msaccess"},
        {".mdb", "application/x-msaccess"},
        {".mde", "application/msaccess"},
        {".mdp", "application/octet-stream"},
        {".me", "application/x-troff-me"},
        {".mfp", "application/x-shockwave-flash"},
        {".mht", "message/rfc822"},
        {".mhtml", "message/rfc822"},
        {".mid", "audio/mid"},
        {".midi", "audio/mid"},
        {".mix", "application/octet-stream"},
        {".mk", "text/plain"},
        {".mmf", "application/x-smaf"},
        {".mno", "text/xml"},
        {".mny", "application/x-msmoney"},
        {".mod", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".movie", "video/x-sgi-movie"},
        {".mp2", "video/mpeg"},
        {".mp2v", "video/mpeg"},
        {".mp3", "audio/mpeg"},
        {".mp4", "video/mp4"},
        {".mp4v", "video/mp4"},
        {".mpa", "video/mpeg"},
        {".mpe", "video/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpf", "application/vnd.ms-mediapackage"},
        {".mpg", "video/mpeg"},
        {".mpp", "application/vnd.ms-project"},
        {".mpv2", "video/mpeg"},
        {".mqv", "video/quicktime"},
        {".ms", "application/x-troff-ms"},
        {".msi", "application/octet-stream"},
        {".mso", "application/octet-stream"},
        {".mts", "video/vnd.dlna.mpeg-tts"},
        {".mtx", "application/xml"},
        {".mvb", "application/x-msmediaview"},
        {".mvc", "application/x-miva-compiled"},
        {".mxp", "application/x-mmxp"},
        {".nc", "application/x-netcdf"},
        {".nsc", "video/x-ms-asf"},
        {".nws", "message/rfc822"},
        {".ocx", "application/octet-stream"},
        {".oda", "application/oda"},
        {".odc", "text/x-ms-odc"},
        {".odh", "text/plain"},
        {".odl", "text/plain"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/oleobject"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".one", "application/onenote"},
        {".onea", "application/onenote"},
        {".onepkg", "application/onenote"},
        {".onetmp", "application/onenote"},
        {".onetoc", "application/onenote"},
        {".onetoc2", "application/onenote"},
        {".orderedtest", "application/xml"},
        {".osdx", "application/opensearchdescription+xml"},
        {".p10", "application/pkcs10"},
        {".p12", "application/x-pkcs12"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7c", "application/pkcs7-mime"},
        {".p7m", "application/pkcs7-mime"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7s", "application/pkcs7-signature"},
        {".pbm", "image/x-portable-bitmap"},
        {".pcast", "application/x-podcast"},
        {".pct", "image/pict"},
        {".pcx", "application/octet-stream"},
        {".pcz", "application/octet-stream"},
        {".pdf", "application/pdf"},
        {".pfb", "application/octet-stream"},
        {".pfm", "application/octet-stream"},
        {".pfx", "application/x-pkcs12"},
        {".pgm", "image/x-portable-graymap"},
        {".pic", "image/pict"},
        {".pict", "image/pict"},
        {".pkgdef", "text/plain"},
        {".pkgundef", "text/plain"},
        {".pko", "application/vnd.ms-pki.pko"},
        {".pls", "audio/scpls"},
        {".pma", "application/x-perfmon"},
        {".pmc", "application/x-perfmon"},
        {".pml", "application/x-perfmon"},
        {".pmr", "application/x-perfmon"},
        {".pmw", "application/x-perfmon"},
        {".png", "image/png"},
        {".pnm", "image/x-portable-anymap"},
        {".pnt", "image/x-macpaint"},
        {".pntg", "image/x-macpaint"},
        {".pnz", "image/png"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
        {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
        {".ppa", "application/vnd.ms-powerpoint"},
        {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
        {".ppm", "image/x-portable-pixmap"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
        {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".prf", "application/pics-rules"},
        {".prm", "application/octet-stream"},
        {".prx", "application/octet-stream"},
        {".ps", "application/postscript"},
        {".psc1", "application/PowerShell"},
        {".psd", "application/octet-stream"},
        {".psess", "application/xml"},
        {".psm", "application/octet-stream"},
        {".psp", "application/octet-stream"},
        {".pub", "application/x-mspublisher"},
        {".pwz", "application/vnd.ms-powerpoint"},
        {".qht", "text/x-html-insertion"},
        {".qhtm", "text/x-html-insertion"},
        {".qt", "video/quicktime"},
        {".qti", "image/x-quicktime"},
        {".qtif", "image/x-quicktime"},
        {".qtl", "application/x-quicktimeplayer"},
        {".qxd", "application/octet-stream"},
        {".ra", "audio/x-pn-realaudio"},
        {".ram", "audio/x-pn-realaudio"},
        {".rar", "application/octet-stream"},
        {".ras", "image/x-cmu-raster"},
        {".rat", "application/rat-file"},
        {".rc", "text/plain"},
        {".rc2", "text/plain"},
        {".rct", "text/plain"},
        {".rdlc", "application/xml"},
        {".resx", "application/xml"},
        {".rf", "image/vnd.rn-realflash"},
        {".rgb", "image/x-rgb"},
        {".rgs", "text/plain"},
        {".rm", "application/vnd.rn-realmedia"},
        {".rmi", "audio/mid"},
        {".rmp", "application/vnd.rn-rn_music_package"},
        {".roff", "application/x-troff"},
        {".rpm", "audio/x-pn-realaudio-plugin"},
        {".rqy", "text/x-ms-rqy"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".ruleset", "application/xml"},
        {".s", "text/plain"},
        {".safariextz", "application/x-safari-safariextz"},
        {".scd", "application/x-msschedule"},
        {".sct", "text/scriptlet"},
        {".sd2", "audio/x-sd2"},
        {".sdp", "application/sdp"},
        {".sea", "application/octet-stream"},
        {".searchConnector-ms", "application/windows-search-connector+xml"},
        {".setpay", "application/set-payment-initiation"},
        {".setreg", "application/set-registration-initiation"},
        {".settings", "application/xml"},
        {".sgimb", "application/x-sgimb"},
        {".sgml", "text/sgml"},
        {".sh", "application/x-sh"},
        {".shar", "application/x-shar"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".sitemap", "application/xml"},
        {".skin", "application/xml"},
        {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
        {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
        {".slk", "application/vnd.ms-excel"},
        {".sln", "text/plain"},
        {".slupkg-ms", "application/x-ms-license"},
        {".smd", "audio/x-smd"},
        {".smi", "application/octet-stream"},
        {".smx", "audio/x-smd"},
        {".smz", "audio/x-smd"},
        {".snd", "audio/basic"},
        {".snippet", "application/xml"},
        {".snp", "application/octet-stream"},
        {".sol", "text/plain"},
        {".sor", "text/plain"},
        {".spc", "application/x-pkcs7-certificates"},
        {".spl", "application/futuresplash"},
        {".src", "application/x-wais-source"},
        {".srf", "text/plain"},
        {".SSISDeploymentManifest", "text/xml"},
        {".ssm", "application/streamingmedia"},
        {".sst", "application/vnd.ms-pki.certstore"},
        {".stl", "application/vnd.ms-pki.stl"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".sv4crc", "application/x-sv4crc"},
        {".svc", "application/xml"},
        {".swf", "application/x-shockwave-flash"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tcl", "application/x-tcl"},
        {".testrunconfig", "application/xml"},
        {".testsettings", "application/xml"},
        {".tex", "application/x-tex"},
        {".texi", "application/x-texinfo"},
        {".texinfo", "application/x-texinfo"},
        {".tgz", "application/x-compressed"},
        {".thmx", "application/vnd.ms-officetheme"},
        {".thn", "application/octet-stream"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".tlh", "text/plain"},
        {".tli", "text/plain"},
        {".toc", "application/octet-stream"},
        {".tr", "application/x-troff"},
        {".trm", "application/x-msterminal"},
        {".trx", "application/xml"},
        {".ts", "video/vnd.dlna.mpeg-tts"},
        {".tsv", "text/tab-separated-values"},
        {".ttf", "application/octet-stream"},
        {".tts", "video/vnd.dlna.mpeg-tts"},
        {".txt", "text/plain"},
        {".u32", "application/octet-stream"},
        {".uls", "text/iuls"},
        {".user", "text/plain"},
        {".ustar", "application/x-ustar"},
        {".vb", "text/plain"},
        {".vbdproj", "text/plain"},
        {".vbk", "video/mpeg"},
        {".vbproj", "text/plain"},
        {".vbs", "text/vbscript"},
        {".vcf", "text/x-vcard"},
        {".vcproj", "Application/xml"},
        {".vcs", "text/plain"},
        {".vcxproj", "Application/xml"},
        {".vddproj", "text/plain"},
        {".vdp", "text/plain"},
        {".vdproj", "text/plain"},
        {".vdx", "application/vnd.ms-visio.viewer"},
        {".vml", "text/xml"},
        {".vscontent", "application/xml"},
        {".vsct", "text/xml"},
        {".vsd", "application/vnd.visio"},
        {".vsi", "application/ms-vsi"},
        {".vsix", "application/vsix"},
        {".vsixlangpack", "text/xml"},
        {".vsixmanifest", "text/xml"},
        {".vsmdi", "application/xml"},
        {".vspscc", "text/plain"},
        {".vss", "application/vnd.visio"},
        {".vsscc", "text/plain"},
        {".vssettings", "text/xml"},
        {".vssscc", "text/plain"},
        {".vst", "application/vnd.visio"},
        {".vstemplate", "text/xml"},
        {".vsto", "application/x-ms-vsto"},
        {".vsw", "application/vnd.visio"},
        {".vsx", "application/vnd.visio"},
        {".vtx", "application/vnd.visio"},
        {".wav", "audio/wav"},
        {".wave", "audio/wav"},
        {".wax", "audio/x-ms-wax"},
        {".wbk", "application/msword"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wcm", "application/vnd.ms-works"},
        {".wdb", "application/vnd.ms-works"},
        {".wdp", "image/vnd.ms-photo"},
        {".webarchive", "application/x-safari-webarchive"},
        {".webtest", "application/xml"},
        {".wiq", "application/xml"},
        {".wiz", "application/msword"},
        {".wks", "application/vnd.ms-works"},
        {".WLMP", "application/wlmoviemaker"},
        {".wlpginstall", "application/x-wlpg-detect"},
        {".wlpginstall3", "application/x-wlpg3-detect"},
        {".wm", "video/x-ms-wm"},
        {".wma", "audio/x-ms-wma"},
        {".wmd", "application/x-ms-wmd"},
        {".wmf", "application/x-msmetafile"},
        {".wml", "text/vnd.wap.wml"},
        {".wmlc", "application/vnd.wap.wmlc"},
        {".wmls", "text/vnd.wap.wmlscript"},
        {".wmlsc", "application/vnd.wap.wmlscriptc"},
        {".wmp", "video/x-ms-wmp"},
        {".wmv", "video/x-ms-wmv"},
        {".wmx", "video/x-ms-wmx"},
        {".wmz", "application/x-ms-wmz"},
        {".wpl", "application/vnd.ms-wpl"},
        {".wps", "application/vnd.ms-works"},
        {".wri", "application/x-mswrite"},
        {".wrl", "x-world/x-vrml"},
        {".wrz", "x-world/x-vrml"},
        {".wsc", "text/scriptlet"},
        {".wsdl", "text/xml"},
        {".wvx", "video/x-ms-wvx"},
        {".x", "application/directx"},
        {".xaf", "x-world/x-vrml"},
        {".xaml", "application/xaml+xml"},
        {".xap", "application/x-silverlight-app"},
        {".xbap", "application/x-ms-xbap"},
        {".xbm", "image/x-xbitmap"},
        {".xdr", "text/plain"},
        {".xht", "application/xhtml+xml"},
        {".xhtml", "application/xhtml+xml"},
        {".xla", "application/vnd.ms-excel"},
        {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
        {".xlc", "application/vnd.ms-excel"},
        {".xld", "application/vnd.ms-excel"},
        {".xlk", "application/vnd.ms-excel"},
        {".xll", "application/vnd.ms-excel"},
        {".xlm", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
        {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".xlt", "application/vnd.ms-excel"},
        {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
        {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
        {".xlw", "application/vnd.ms-excel"},
        {".xml", "text/xml"},
        {".xmta", "application/xml"},
        {".xof", "x-world/x-vrml"},
        {".XOML", "text/plain"},
        {".xpm", "image/x-xpixmap"},
        {".xps", "application/vnd.ms-xpsdocument"},
        {".xrm-ms", "text/xml"},
        {".xsc", "application/xml"},
        {".xsd", "text/xml"},
        {".xsf", "text/xml"},
        {".xsl", "text/xml"},
        {".xslt", "text/xml"},
        {".xsn", "application/octet-stream"},
        {".xss", "application/xml"},
        {".xtp", "application/octet-stream"},
        {".xwd", "image/x-xwindowdump"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
        #endregion        
      };

      public ImageUploader(OAuthToken token)
      {
         _oauthToken = token;

         var site = new SiteEntity(_oauthToken);
         var user = site.GetAuthenticatedUserAsync().Result;

         var templates = user.GetAlbumTemplatesAsync().Result;

         _noBuyTemplate = templates.Single(t => t.Name == "NoBuy");
         _defaultTemplate = templates.Single(t => t.Name == "KHainePhotography");
         _rootNode = user.GetRootNodeAsync().Result;
         CustomersFolder = _rootNode.GetChildrenAsync(type: TypeEnum.Folder).Result.Single(f => f.Name == "Customers");
      }

      public NodeEntity GetSubNode(NodeEntity parentFolder, string subFolderName, TypeEnum nodeType, bool cached = true, string subFolderPassword = "NONE")
      {
         return GetSubNode(parentFolder, subFolderName, nodeType, out _, cached, subFolderPassword);
      }

      public NodeEntity GetSubNode(NodeEntity parentFolder, string subFolderName, TypeEnum nodeType, out bool haveCreated, bool cached = true, string subFolderPassword = "NONE")
      {
         var key = $"{parentFolder.Key}.{subFolderName}";
         haveCreated = false;
         if (cached && _cache.ContainsKey(key))
         {
            ConsolePrinter.Write(ConsoleColor.Green, $"Loaded {nodeType} : {subFolderName} (cached)");
            return _cache[key];
         }

         var subFolder = parentFolder.GetChildrenAsync(type: nodeType).Result?.FirstOrDefault(f => string.Equals(f.Name, subFolderName, StringComparison.CurrentCultureIgnoreCase));

         if (subFolder != null)
         {
            subFolder.Key = key;
            if (!_cache.ContainsKey(key))
               _cache.Add(key, subFolder);
            else
               _cache[key] = subFolder;

            ConsolePrinter.Write(ConsoleColor.Green, $"Loaded {nodeType} : {subFolderName}");
            return subFolder;
         }

         ConsolePrinter.Write(ConsoleColor.DarkYellow, $"Creating {nodeType} : {subFolderName}");

         //we need to create it
         subFolder = new NodeEntity(_oauthToken)
         {
            Type = nodeType,
            Description = subFolderName,
            Name = subFolderName,
            UrlName = subFolderName,
            Keywords = new[] { subFolderName },
            Parent = parentFolder,
            Privacy = subFolderPassword == "NONE" ? PrivacyEnum.Public : PrivacyEnum.Unlisted,
            SecurityType = subFolderPassword == "NONE" ? SecurityTypeEnum.None : SecurityTypeEnum.Password,
            Password = subFolderPassword == "NONE" ? null : subFolderPassword,
            PasswordHint = subFolderPassword == "NONE" ? null : "It's on your sales reciept",
            Key = key
         };

         subFolder.CreateAsync(parentFolder).Wait();
         haveCreated = true;

         //Read it back in so we got all properties
         subFolder = parentFolder.GetChildrenAsync(type: nodeType).Result?.FirstOrDefault(f => string.Equals(f.Name, subFolderName, StringComparison.CurrentCultureIgnoreCase));
         subFolder.Key = key;

         _cache.Add(key, subFolder);

         return subFolder;
      }

      public string ProcessImages(
         string customerName,
         string customerPassword,
         string shootName,
         List<string> originals,
         List<string> videos,
         List<string> colours,
         List<string> sepias,
         List<string> bandWs)
      {

         if (string.IsNullOrWhiteSpace(customerName))
         {
            ConsolePrinter.Write(ConsoleColor.Red, "Customer Name not specfied!");
            return null;
         }

         if (string.IsNullOrWhiteSpace(customerPassword))
         {
            ConsolePrinter.Write(ConsoleColor.Red, $"User: {customerName} Password not specfied!");
            return null;
         }

         if (string.IsNullOrWhiteSpace(shootName))
         {
            ConsolePrinter.Write(ConsoleColor.Red, $"{customerName} Shoot Name not specfied!");
            return null;
         }

         if (!originals.Any() && !videos.Any()  && !colours.Any() && !sepias.Any() && !bandWs.Any())
         {
            ConsolePrinter.Write(ConsoleColor.Red, $"{customerName} {shootName} Nothing to upload!");
            return null;
         }

         var customerFolder = GetSubNode(CustomersFolder, customerName, TypeEnum.Folder, true, customerPassword);

         if (customerFolder == null)
         {
            ConsolePrinter.Write(ConsoleColor.Red, $"{customerName} : No Customer Folder - Nothing we can do!");
            return null;
         }

         if (customerName == "Haine")
         {
            shootName = shootName.Substring(0, 4);
         }

         var shootFolder = GetSubNode(customerFolder, shootName, TypeEnum.Folder);
         Upload(shootFolder, "Original", originals, true);
         Upload(shootFolder, "Video", videos, true);

         var editsFolder = GetSubNode(shootFolder, "Edits", TypeEnum.Folder);
         Upload(editsFolder, "Colour", colours, true);
         Upload(editsFolder, "Sepia", sepias, true);
         Upload(editsFolder, "BandW", bandWs, true);

         return customerFolder.WebUri;
      }

      public void Upload(NodeEntity parentFolder, string albumName, List<string> files, bool doFileCountCheck)
      {
         if (files?.Any() != true)
         {
            return;
         }
               
         try
         {
            var albumNode = GetSubNode(parentFolder, albumName, TypeEnum.Album, out bool addCopyright);

            if (addCopyright && files.Any(f => f.ToLower().EndsWith(".jpg")))
            {
               files.Add(@"\\khpserver\Documents\CopyrightRelease.JPG");
            }

            var customerId = albumNode.Key.Split('.')[1];

            ConsolePrinter.Write(ConsoleColor.White, $"Uploading {files.Count()} images to {customerId} {albumName}.");

            var albumUri = albumNode.Uris.Single(uri => uri.Key == "Album").Value.Uri;
            var album = AlbumEntity.RetrieveEntityAsync<AlbumEntity>(_oauthToken, $"{Constants.Addresses.SmugMugApi}{albumUri}").Result;

            var template = albumName == "Original" || albumName == "Video" ? _noBuyTemplate : _defaultTemplate;

            if (album.TemplateUri != template.TemplateUri)
            {
               ConsolePrinter.Write(ConsoleColor.Yellow, $"Fixing album template on {albumNode.Key}.");
               album.ApplyAlbumTemplateAsync(template).Wait();
            }
            else
            {
               ConsolePrinter.Write(ConsoleColor.Green, $"Album template on {albumNode.Key} is already set to {template.Name}.");
            }

            var existingItems = album.GetImagesAsync().Result?.Select(i => i.FileName).ToList();
            var existingItemCount = existingItems?.Count ?? 0;

            if (doFileCountCheck && files.Count() > 1 && existingItemCount >= files.Count())
            {
               ConsolePrinter.Write(ConsoleColor.White, $"No folder upload action required Folder counts match have {files.Count()} exist {existingItemCount}.");
               return;
            }

            ConsolePrinter.Write(ConsoleColor.White, $"{albumNode.Key} {existingItemCount} exist, {files.Count()} to upload.");

            OAuth.OAuthMessageHandler oAuthhandler = new OAuth.OAuthMessageHandler(
               _oauthToken.ApiKey,
               _oauthToken.Secret,
               _oauthToken.Token,
               _oauthToken.TokenSecret);

            using (var client = new HttpClient(oAuthhandler))
            {
               client.Timeout = TimeSpan.FromDays(1);
               client.DefaultRequestHeaders.Accept.Clear();
               client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

               AddUploadHeaders(client, albumUri);

               var count = 0;
               var dataToProcessKb = files.Select(f => new FileInfo(f).Length).Sum() / 1024;
               foreach (var file in files)
               {
                  try
                  {
                     var name = Path.GetFileNameWithoutExtension(file);
                     var fileInfo = new FileInfo(file);
                     dataToProcessKb -= (fileInfo.Length / 1024);
                     count++;

                     if (existingItems != null && existingItems.Any(i => i.StartsWith(name, StringComparison.OrdinalIgnoreCase) || name.StartsWith(i, StringComparison.OrdinalIgnoreCase)))
                     {
                        ConsolePrinter.Write(ConsoleColor.Yellow, $"Skipping {name} it's already there!");
                        continue;
                     }

                     var fileItem = new FileInfo(file);
                     _lastSizeInKb = (int)(fileItem.Length / 1024);
                     var timeout = TimeSpan.FromSeconds((_lastSizeInKb / 100) + 60); //100kb a second is reasonalble + 60 secs to handle when things are sloooooow...)

                     var tokenSource = new CancellationTokenSource();

                     var speed = _lastSpeed == 0 ? 700 : _lastSpeed;
                     var fileDuration = TimeSpan.FromSeconds(_lastSizeInKb / speed);
                     var filesToProcess = files.Count() - count;
                     var etaDuration = TimeSpan.FromSeconds((dataToProcessKb + _lastSizeInKb) / speed);

                     ConsolePrinter.Write(ConsoleColor.Cyan, $"Uploading {fileItem.Name} size-{_lastSizeInKb}kb timeout-{timeout} Duration-{fileDuration:mm\\:ss}");
                     ConsolePrinter.Write(ConsoleColor.Cyan, $"{customerId} {albumName} {filesToProcess} still to process Finish-{DateTime.Now.Add(etaDuration).AddSeconds(filesToProcess * 1.4):T}");

                     var md5CheckSum = GetCheckSum(file);

                     using (var fileContent = new ProgressableStreamContent(this, fileItem.OpenRead(), 1024 * 32))
                     {
                        AddUploadHeaders(fileContent.Headers, fileItem, name, md5CheckSum);

                        (ConsoleColor, string) consoleResult;

                        var resp = client.PostAsync(Constants.Addresses.SmugMugUpload, fileContent, tokenSource.Token);
                        if (resp.Wait(timeout))
                        {
                           consoleResult = ParseReponse(resp.Result);
                        }
                        else
                        {
                           consoleResult = (ConsoleColor.Red, "Timeout");
                           tokenSource.Cancel();
                        }

                        ConsolePrinter.Write(consoleResult);
                     }
                  }
                  catch (Exception e)
                  {
                     ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
                  }
               }
            }
         }
         catch (Exception e)
         {
            ConsolePrinter.Write(ConsoleColor.Red, e.ToString());
         }
      }

      private (ConsoleColor, string) ParseReponse(HttpResponseMessage result)
      {
         if (result.StatusCode == HttpStatusCode.OK)
         {
            var resp = result.Content.ReadAsStringAsync();
            resp.Wait();

            var response = JsonConvert.DeserializeObject<UploadReponse>(Uri.UnescapeDataString(resp.Result));
            if (response.stat != "ok")
            {
               Debug.WriteLine(resp.Result);
               throw new HttpRequestException($"{response.message} : {response.code}");
            }

            return (result.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red, result.StatusCode.ToString());
         }
         else
         {
            var fileName = Path.GetTempFileName() + ".htm";
            var data = System.Text.Encoding.UTF8.GetString(result.Content.ReadAsByteArrayAsync().Result);
            File.WriteAllText(fileName, data);
            Process.Start(fileName);

            return (ConsoleColor.Red, result.ReasonPhrase);
         }
      }


      private string GetCheckSum(string file)
      {
         using (var md5 = MD5.Create())
         {
            using (var stream = File.OpenRead(file))
            {
               var hash = md5.ComputeHash(stream);
               return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
         }
      }

      private void AddUploadHeaders(HttpClient client, string albumUri)
      {
         client.DefaultRequestHeaders.Add("User-Agent", "KHainePhotography_PhotoStudioManager_v1.0");

         client.DefaultRequestHeaders.Add("X-Smug-AlbumUri", albumUri);
         client.DefaultRequestHeaders.Add("X-Smug-ResponseType", "JSON");
         client.DefaultRequestHeaders.Add("X-Smug-Version", "v2");

         client.DefaultRequestHeaders.Add("X-Smug-Latitude", "53.457920");
         client.DefaultRequestHeaders.Add("X-Smug-Longitude", "-1.464252");
         client.DefaultRequestHeaders.Add("X-Smug-Altitude", "86");

         client.DefaultRequestHeaders.Add("X-Smug-Pretty", "true");
         client.DefaultRequestHeaders.Add("X-Smug-Hidden", "false");
      }

      private void AddUploadHeaders(HttpContentHeaders headers, FileInfo file, string name, string md5Checksum)
      {
         headers.Add("Content-MD5", md5Checksum);
         headers.Add("Content-Length", file.Length.ToString());
         headers.ContentType = MediaTypeHeaderValue.Parse(GetMimeType(file.Extension));

         headers.Add("X-Smug-FileName", name);
         headers.Add("X-Smug-Title", name);
         headers.Add("X-Smug-Caption", name);
      }

      public static string GetMimeType(string extension)
      {
         if (extension == null)
         {
            throw new ArgumentNullException("extension");
         }

         if (!extension.StartsWith("."))
         {
            extension = "." + extension;
         }

         string mime;

         return _mappings.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
      }

      public void ChangeState(DownloadState state)
      {
         switch (state)
         {
            case DownloadState.PendingUpload:
               _prevProgress = -1;
               _stopwatch.Restart();
               ConsolePrinter.WriteProgressBar(0);
               break;

            case DownloadState.PendingResponse:
               _stopwatch.Stop();

               var thisSpeed = (_lastSizeInKb / _stopwatch.Elapsed.TotalSeconds);

               if (thisSpeed > 5000)
               {
                  thisSpeed = _lastSpeed;
               }

               if (_lastSpeed == 0)
               {
                  _lastSpeed = thisSpeed;
               }
               else
               {
                  _lastSpeed = (thisSpeed + _lastSpeed) / 2;
               }

               Console.WriteLine("");
               ConsolePrinter.Write(ConsoleColor.White, $"Took {_stopwatch.Elapsed} @ {thisSpeed:F2}kbps");
               break;
         }
      }

      Stopwatch _stopwatch = new Stopwatch();
      int _prevProgress = -1;

      public double PercentComplete
      {
         set
         {
            var progress = (int)(value * 100);
            if (_prevProgress == progress)
               return;

            ConsolePrinter.WriteProgressBar(progress, true);
            _prevProgress = progress;
         }
      }

      private int _lastSizeInKb;
      private double _lastSpeed = 0;

      public class UploadReponse
      {
         public string stat { get; set; }
         public string method { get; set; }
         public string code { get; set; }
         public string message { get; set; }
         public ImageUploadData Image { get; set; }
         public class ImageUploadData
         {
            public string ImageUri { get; set; }
            public string AlbumImageUri { get; set; }
            public string StatusImageReplaceUri { get; set; }
            public string URL { get; set; }
         }
      }

   }

   public enum DownloadState
   {
      PendingUpload,
      Uploading,
      PendingResponse
   }

   public interface IUploader
   {
      double PercentComplete { set; }

      void ChangeState(DownloadState pendingUpload);
   }

   internal class ProgressableStreamContent : HttpContent
   {
      private const int defaultBufferSize = 4096;
      private const int minBuffersSize = 1024;

      private readonly Stream content;
      private readonly int bufferSize;
      private readonly IUploader Uploader;

      private bool contentConsumed;

      public ProgressableStreamContent(IUploader Uploader, Stream content, int bufferSize = defaultBufferSize)
      {
         this.Uploader = Uploader ?? throw new ArgumentNullException("Uploader");
         this.content = content ?? throw new ArgumentNullException("content");

         if (bufferSize < minBuffersSize) // Less than 1kb for uplaoding is crazy!
         {
            throw new ArgumentOutOfRangeException("bufferSize", $"Minimum Buffer Size is {minBuffersSize}");
         }
         else
         {
            this.bufferSize = bufferSize;
         }
      }

      protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
      {
         PrepareContent();

         return Task.Run(() =>
         {
            var buffer = new Byte[bufferSize];
            var size = content.Length;
            var uploaded = 0;

            Uploader.ChangeState(DownloadState.PendingUpload);

            using (content)
               while (true)
               {
                  var length = content.Read(buffer, 0, buffer.Length);
                  if (length <= 0) break;

                  uploaded += length;

                  Uploader.PercentComplete = (double)uploaded / (double)size;

                  stream.Write(buffer, 0, length);

                  Uploader.ChangeState(DownloadState.Uploading);
               }

            Uploader.ChangeState(DownloadState.PendingResponse);
         });
      }

      protected override bool TryComputeLength(out long length)
      {
         length = content.Length;
         return true;
      }

      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            content.Dispose();
         }
         base.Dispose(disposing);
      }

      private void PrepareContent()
      {
         if (contentConsumed)
         {
            if (content.CanSeek)
            {
               content.Position = 0;
            }
            else
            {
               throw new InvalidOperationException("Stream cannot Seek");
            }
         }

         contentConsumed = true;
      }
   }
}
