using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using Steamworks;

namespace PEAKLib.Networking.Modules
{
    /// <summary>
    /// Message format
    /// </summary>
    public class Message : IDisposable
    {
        /// <summary>
        /// </summary>
        public const byte PROTOCOL_VERSION = 1;
        /// <summary>
        /// </summary>
        public static int MaxSize = 64 * 1024; // default; adapter may set to Steam max

        /// <summary>
        /// </summary>
        public byte ProtocolVersion;
        /// <summary>
        /// </summary>
        public uint ModID;
        /// <summary>
        /// </summary>
        public string MethodName = string.Empty;
        /// <summary>
        /// </summary>
        public int Mask;

        private List<byte> buffer = new();
        private byte[] readableBuffer = Array.Empty<byte>();
        private int readPos = 0;

        /// <summary>
        /// </summary>
        public bool Compressed { get; private set; } = false;

        /// <summary>
        /// </summary>
        public Message(uint modId, string methodName, int mask)
        {
            ProtocolVersion = PROTOCOL_VERSION;
            ModID = modId;
            MethodName = methodName;
            Mask = mask;

            WriteByte(ProtocolVersion);
            WriteUInt(ModID);
            WriteString(MethodName);
            WriteInt(Mask);
        }

        /// <summary>
        /// </summary>
        public Message(byte[] data)
        {
            SetBytes(data);
            ProtocolVersion = ReadByte();
            ModID = ReadUInt();
            MethodName = ReadString();
            Mask = ReadInt();

            // Add a flag to indicate compression though it is handled itself by the adapter.
        }

        /// <summary>
        /// </summary>
        public void SetBytes(byte[] data)
        {
            buffer.Clear();
            buffer.AddRange(data);
            readableBuffer = buffer.ToArray();
            readPos = 0;
        }

        /// <summary>
        /// </summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>
        /// </summary>
        public int Length() => buffer.Count;
        /// <summary>
        /// </summary>
        public int UnreadLength() => Length() - readPos;

        /// <summary>
        /// </summary>
        public void Reset(bool zero = true)
        {
            if (zero)
            {
                buffer.Clear();
                readableBuffer = Array.Empty<byte>();
                readPos = 0;
            }
            else
            {
                readPos -= 4;
            }
        }

        #region Write helpers
        /// <summary>
        /// </summary>
        public Message WriteByte(byte v) { buffer.Add(v); return this; }
        /// <summary>
        /// </summary>
        public Message WriteBytes(byte[] v) { WriteInt(v.Length); buffer.AddRange(v); return this; }
        /// <summary>
        /// </summary>
        public Message WriteInt(int v) { buffer.AddRange(BitConverter.GetBytes(v)); return this; }
        /// <summary>
        /// </summary>
        public Message WriteUInt(uint v) { buffer.AddRange(BitConverter.GetBytes(v)); return this; }
        /// <summary>
        /// </summary>
        public Message WriteLong(long v) { buffer.AddRange(BitConverter.GetBytes(v)); return this; }
        /// <summary>
        /// </summary>
        public Message WriteULong(ulong v) { buffer.AddRange(BitConverter.GetBytes(v)); return this; }
        /// <summary>
        /// </summary>
        public Message WriteFloat(float v) { buffer.AddRange(BitConverter.GetBytes(v)); return this; }
        /// <summary>
        /// </summary>
        public Message WriteBool(bool v) { buffer.AddRange(BitConverter.GetBytes(v)); return this; }
        /// <summary>
        /// </summary>
        public Message WriteString(string v)
        {
            var bytes = Encoding.UTF8.GetBytes(v ?? "");
            WriteInt(bytes.Length);
            buffer.AddRange(bytes);
            return this;
        }
        /// <summary>
        /// </summary>
        public Message WriteVector3(Vector3 v) { WriteFloat(v.x); WriteFloat(v.y); WriteFloat(v.z); return this; }
        /// <summary>
        /// </summary>
        public Message WriteQuaternion(Quaternion q) { WriteFloat(q.x); WriteFloat(q.y); WriteFloat(q.z); WriteFloat(q.w); return this; }

        /// <summary>
        /// </summary>
        public void WriteObject(Type type, object value)
        {
            if (!writeCasters.TryGetValue(type, out var w)) throw new Exception($"Unsupported type {type}");
            w(this, value);
        }

        private static readonly Dictionary<Type, Action<Message, object>> writeCasters = new()
        {
            { typeof(byte), (m,o) => m.WriteByte((byte)o) },
            { typeof(byte[]), (m,o) => m.WriteBytes((byte[])o) },
            { typeof(int), (m,o) => m.WriteInt((int)o) },
            { typeof(uint), (m,o) => m.WriteUInt((uint)o) },
            { typeof(long), (m,o) => m.WriteLong((long)o) },
            { typeof(ulong), (m,o) => m.WriteULong((ulong)o) },
            { typeof(float), (m,o) => m.WriteFloat((float)o) },
            { typeof(bool), (m,o) => m.WriteBool((bool)o) },
            { typeof(string), (m,o) => m.WriteString((string)o) },
            { typeof(Vector3), (m,o) => m.WriteVector3((Vector3)o) },
            { typeof(Quaternion), (m,o) => m.WriteQuaternion((Quaternion)o) },
            { typeof(CSteamID), (m,o) => m.WriteULong(((CSteamID)o).m_SteamID) },
        };
        #endregion

        #region Read helpers
        /// <summary>
        /// </summary>
        public byte ReadByte()
        {
            if (buffer.Count > readPos) { byte v = readableBuffer[readPos]; readPos++; return v; }
            throw new Exception("ReadByte out of range");
        }
        /// <summary>
        /// </summary>
        public int ReadInt()
        {
            if (buffer.Count > readPos) { int v = BitConverter.ToInt32(readableBuffer, readPos); readPos += 4; return v; }
            throw new Exception("ReadInt out of range");
        }
        /// <summary>
        /// </summary>
        public uint ReadUInt()
        {
            if (buffer.Count > readPos) { uint v = BitConverter.ToUInt32(readableBuffer, readPos); readPos += 4; return v; }
            throw new Exception("ReadUInt out of range");
        }
        /// <summary>
        /// </summary>
        public long ReadLong()
        {
            if (buffer.Count > readPos) { long v = BitConverter.ToInt64(readableBuffer, readPos); readPos += 8; return v; }
            throw new Exception("ReadLong out of range");
        }
        /// <summary>
        /// </summary>
        public ulong ReadULong()
        {
            if (buffer.Count > readPos) { ulong v = BitConverter.ToUInt64(readableBuffer, readPos); readPos += 8; return v; }
            throw new Exception("ReadULong out of range");
        }
        /// <summary>
        /// </summary>
        public float ReadFloat()
        {
            if (buffer.Count > readPos) { float v = BitConverter.ToSingle(readableBuffer, readPos); readPos += 4; return v; }
            throw new Exception("ReadFloat out of range");
        }
        /// <summary>
        /// </summary>
        public bool ReadBool()
        {
            if (buffer.Count > readPos) { bool v = BitConverter.ToBoolean(readableBuffer, readPos); readPos += 1; return v; }
            throw new Exception("ReadBool out of range");
        }
        /// <summary>
        /// </summary>
        public string ReadString()
        {
            int len = ReadInt();
            if (len == 0) return string.Empty;
            if (buffer.Count > readPos)
            {
                string s = Encoding.UTF8.GetString(readableBuffer, readPos, len);
                readPos += len;
                return s;
            }
            throw new Exception("ReadString out of range");
        }
        /// <summary>
        /// </summary>
        public Vector3 ReadVector3() => new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        /// <summary>
        /// </summary>
        public Quaternion ReadQuaternion() => new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());

        /// <summary>
        /// </summary>
        public object ReadObject(Type type)
        {
            if (!readCasters.TryGetValue(type, out var r)) throw new Exception($"Unsupported read type {type}");
            return r(this);
        }

        private static readonly Dictionary<Type, Func<Message, object>> readCasters = new()
        {
            { typeof(byte), (m) => m.ReadByte() },
            { typeof(byte[]), (m) => { int l = m.ReadInt(); if (l==0) return new byte[0]; var arr = new byte[l]; Array.Copy(m.readableBuffer, m.readPos, arr, 0, l); m.readPos += l; return arr; } },
            { typeof(int), (m) => m.ReadInt() },
            { typeof(uint), (m) => m.ReadUInt() },
            { typeof(long), (m) => m.ReadLong() },
            { typeof(ulong), (m) => m.ReadULong() },
            { typeof(float), (m) => m.ReadFloat() },
            { typeof(bool), (m) => m.ReadBool() },
            { typeof(string), (m) => m.ReadString() },
            { typeof(Vector3), (m) => m.ReadVector3() },
            { typeof(Quaternion), (m) => m.ReadQuaternion() },
            { typeof(CSteamID), (m) => new CSteamID(m.ReadULong()) },
        };
        #endregion

        #region Compression helpers
        /// <summary>
        /// </summary>
        public byte[] CompressPayload()
        {
            var data = ToArray();
            using var ms = new MemoryStream();
            using (var gz = new GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal, true))
            {
                gz.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// </summary>
        public static byte[] DecompressPayload(byte[] compressed)
        {
            using var ms = new MemoryStream(compressed);
            using var gz = new GZipStream(ms, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            gz.CopyTo(outMs);
            return outMs.ToArray();
        }
        #endregion

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            buffer = null!;
            readableBuffer = null!;
            GC.SuppressFinalize(this);
        }
    }
}
