﻿using IziHardGames.Libs.NonEngine.Memory;
using System.IO.Pipelines;
using System.Net.Sockets;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class PipeFactory
    {
        public static PipeWrap Wrap(NetworkStream ns)
        {
            var item = PoolObjectsConcurent<PipeWrap>.Shared.Rent();
            item.Wrap(ns);
            return item;
        }
    }

    public class PipeWrap
    {
        public Stream innerStream;
        public PipeReader reader;
        public PipeWriter writer;
        public Pipe pipe;


        public void Wrap(NetworkStream networkStream)
        {

        }
    }

    public class PipeClient
    {

    }

    public class PipeServer
    {

    }
}