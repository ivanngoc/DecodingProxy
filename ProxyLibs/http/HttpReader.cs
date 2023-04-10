// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Net.Sockets;
using static System.Net.Mime.MediaTypeNames;

namespace HttpDecodingProxy.http
{

    public class HttpReader
    {
        public static void Test1(NetworkStream stream)
        {
            //  GET http://www.columbia.edu/~fdc/sample.html HTTP/1.1
            //  Host: www.columbia.edu
            //  User - Agent: Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 109.0) Gecko / 20100101 Firefox / 111.0
            //  Accept: text / html,application / xhtml + xml,application / xml; q = 0.9,image / avif,image / webp,*/*;q=0.8
            //  Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3
            //  Accept-Encoding: gzip, deflate
            //  Connection: keep-alive
            //  Upgrade-Insecure-Requests: 1
            //  (CRLF) is must 

            while (true)
            {
                int i = stream.ReadByte();
                if (i < 0) break;
                Console.Write((char)i);
            }
        }
        public static void Test()
        {
            TcpListener listener = new TcpListener(60121);
            listener.Start();
        }
    }

    class GetBinary
    {
        //GET http://detectportal.firefox.com/canonical.html HTTP/1.1
        //Host: detectportal.firefox.com
        //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0
        //Accept: */*
        //Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3
        //Accept-Encoding: gzip, deflate
        //Cache-Control: no-cache
        //Pragma: no-cache
        //Connection: keep-alive

        //HTTP/1.1 200 OK
        //Server: nginx
        //Content-Length: 90
        //Via: 1.1 google
        //Date: Tue, 28 Mar 2023 02:08:54 GMT
        //Content-Type: text/html
        //Age: 16062
        //Cache-Control: public,must-revalidate,max-age=0,s-maxage=3600

        //<meta http-equiv="refresh" content="0;url=https://support.mozilla.org/kb/captive-portal"/>
    }
}