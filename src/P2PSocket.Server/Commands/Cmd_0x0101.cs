﻿using P2PSocket.Core.Commands;
using P2PSocket.Core.Models;
using P2PSocket.Core.Extends;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using P2PSocket.Server.Models.Send;
using P2PSocket.Core.Utils;
using P2PSocket.Server.Models;
using System.Linq;

namespace P2PSocket.Server.Commands
{
    [CommandFlag(Core.P2PCommandType.Login0x0101)]
    public class Cmd_0x0101 : P2PCommand
    {
        readonly P2PTcpClient m_tcpClient;
        BinaryReader m_data { get; }
        public Cmd_0x0101(P2PTcpClient tcpClient, byte[] data)
        {
            m_tcpClient = tcpClient;
            m_data = new BinaryReader(new MemoryStream(data));
        }
        public override bool Excute()
        {
            string clientName = BinaryUtils.ReadString(m_data);
            string authCode = BinaryUtils.ReadString(m_data);
            if (Global.ClientAuthList.Count == 0 || Global.ClientAuthList.Any(t => t.Match(clientName, authCode)))
            {
                bool isSuccess = true;
                P2PTcpItem item = new P2PTcpItem();
                item.TcpClient = m_tcpClient;
                if (Global.TcpMap.ContainsKey(clientName))
                {
                    if (Global.TcpMap[clientName].TcpClient.Connected)
                    {
                        isSuccess = false;
                        Send_0x0101 sendPacket = new Send_0x0101(m_tcpClient, false, $"ClientName:{clientName} 已被使用");
                        m_tcpClient.Client.Send(sendPacket.PackData());
                        m_tcpClient.Close();
                    }
                    else
                    {
                        Global.TcpMap[clientName] = item;
                    }
                }
                else
                    Global.TcpMap.Add(clientName, item);
                if (isSuccess)
                {
                    m_tcpClient.ClientName = clientName;
                    Send_0x0101 sendPacket = new Send_0x0101(m_tcpClient, true, $"客户端{clientName}认证通过");
                    m_tcpClient.Client.Send(sendPacket.PackData());
                }
            }
            else
            {
                Send_0x0101 sendPacket = new Send_0x0101(m_tcpClient, false, $"客户端{clientName}认证失败");
                m_tcpClient.Client.Send(sendPacket.PackData());
                m_tcpClient.Close();
            }

            return true;
        }
    }
}
