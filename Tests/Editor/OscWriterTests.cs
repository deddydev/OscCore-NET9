﻿using System;
using System.Net;
using NUnit.Framework;
using UnityEngine;

namespace OscCore.Tests
{
    public class OscWriterTests
    {
        const string bpmAddress = "/composition/bpm";
        
        readonly OscWriter m_Writer = new OscWriter();

        int m_WriterLengthBefore;

        [SetUp]
        public void BeforeEach()
        {
            m_Writer.Reset();
            m_WriterLengthBefore = m_Writer.Length;
        }

        [TestCase(130)]
        [TestCase(144)]
        public void WriteInt32(int value)
        {
            m_Writer.Write(value);

            Assert.AreEqual(m_WriterLengthBefore + 4, m_Writer.Length);
            // this tests both that it wrote to the right place in the buffer as well as that the value is right
            var convertedBack = BitConverter.ToInt32(m_Writer.Buffer, m_WriterLengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(50000000)]
        [TestCase(144 * 100000)]
        public void WriteInt64(long value)
        {
            m_Writer.Write(value);
            Assert.AreEqual(m_WriterLengthBefore + 8, m_Writer.Length);
            var bigEndian = BitConverter.ToInt64(m_Writer.Buffer, m_WriterLengthBefore);
            var convertedBack = IPAddress.NetworkToHostOrder(bigEndian);
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(0.00001f)]
        [TestCase(0.867924529f)]
        [TestCase(144f)]
        public void WriteFloat32(float value)
        {
            m_Writer.Write(value);
            Assert.AreEqual(m_WriterLengthBefore + 4, m_Writer.Length);
            var convertedBack = BitConverter.ToSingle(m_Writer.Buffer, m_WriterLengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(0.00000001d)]
        [TestCase(0.8279245299754d)]
        [TestCase(144.1d * 1000d)]
        public void WriteFloat64(double value)
        {
            m_Writer.Write(value);
            Assert.AreEqual(m_WriterLengthBefore + 8, m_Writer.Length);
            var convertedBack = BitConverter.ToDouble(m_Writer.Buffer, m_WriterLengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(50, 100, 0, 255)]
        [TestCase(120, 80, 255, 100)]
        [TestCase(255, 150, 50, 255)]
        public void WriteColor32(byte r, byte g, byte b, byte a)
        {
            var value = new Color32(r, g, b, a);
            m_Writer.Write(value);

            Assert.AreEqual(m_WriterLengthBefore + 4, m_Writer.Length);
            var bR = m_Writer.Buffer[m_WriterLengthBefore + 3];
            var bG = m_Writer.Buffer[m_WriterLengthBefore + 2];
            var bB = m_Writer.Buffer[m_WriterLengthBefore + 1];
            var bA = m_Writer.Buffer[m_WriterLengthBefore];
            var convertedBack = new Color32(bR, bG, bB, bA);
            Assert.AreEqual(value, convertedBack);
        }
        
        [Test]
        public void WriteMidi()
        {
            var value = new MidiMessage(1, 4, 16, 80);
            m_Writer.Write(value);

            Assert.AreEqual(m_WriterLengthBefore + 4, m_Writer.Length);
            var convertedBack = new MidiMessage(m_Writer.Buffer, m_WriterLengthBefore);
            Assert.True(value == convertedBack);
        }
        
        [TestCase('S')]
        [TestCase('m')]
        [TestCase('C')]
        public void WriteAsciiChar(char chr)
        {
            m_Writer.WriteAsciiChar(chr);

            Assert.AreEqual(m_WriterLengthBefore + 4, m_Writer.Length);
            var convertedBack = (char) m_Writer.Buffer[m_WriterLengthBefore + 3];
            Assert.True(chr == convertedBack);
        }
        
        [TestCase(32)]
        [TestCase(43)]
        [TestCase(144)]
        public void WriteBlob(int size)
        {
            var bytes = RandomBytes(size);
            m_Writer.Write(bytes, size);

            var blobContentIndex = m_WriterLengthBefore + 4;
            var blobWriteEndIndex = blobContentIndex + size;
            // was the blob size written properly ?
            var writtenSize = BitConverter.ToInt32(m_Writer.Buffer, m_WriterLengthBefore).ReverseBytes();
            Assert.AreEqual(size, writtenSize);
            Assert.AreEqual(m_WriterLengthBefore + 4 + size, m_Writer.Length);

            // was the blob written the same as the source ?
            for (int i = blobContentIndex; i < blobWriteEndIndex; i++)
            {
                Assert.AreEqual(bytes[i - blobContentIndex], m_Writer.Buffer[i]);
            }
        }

        static byte[] RandomBytes(int count)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte) UnityEngine.Random.Range(0, 255);

            return bytes;
        }
    }
}