using System;
using System.IO;

namespace BlackPackageImageTool
{
    internal class Packer
    {
        private byte[] m_input;
        private MemoryStream m_output;
        private int m_width;
        private int m_height;
        private int m_type;
        int m_stride;
        
        public Packer(byte[] input, int width, int height, int type)
        {
            m_input = input;
            m_width = width;
            m_height = height;
            m_output = new MemoryStream();
            m_type = type;
            m_stride = m_width * 3;
        }
        
        public byte[] Pack()
        {
            switch (m_type)
            {
                case 3: PackV3(null); break;
                case 2: PackV2(); break;
                case 1: PackV1(); break;
                case 0: PackV0(); break;
            }

            return m_output.ToArray();
        }

        public byte[] PackV0()
        {
            // We'll implement a simple packer that just writes data in a way
            // that the V0 unpacker can read it back
            using (var writer = new BinaryWriter(m_output))
            {
                int src = 0;
                while (src < m_input.Length)
                {
                    // Write control byte (for up to 8 sequences)
                    byte control = 0;
                    int controlPos = (int)m_output.Position;
                    writer.Write(control);
                    
                    // Process up to 8 bytes/sequences
                    for (int bit = 0; bit < 8 && src < m_input.Length; bit++)
                    {
                        // For simplicity, we'll just store literal bytes
                        // Set the corresponding bit in control byte
                        control |= (byte)(1 << bit);
                        writer.Write(m_input[src++]);
                    }
                    
                    // Go back and update control byte
                    m_output.Position = controlPos;
                    writer.Write(control);
                    m_output.Position = m_output.Length;
                }
            }
            
            return m_output.ToArray();
        }

        public byte[] PackV1()
        {
            using (var writer = new BinaryWriter(m_output))
            {
                byte[] frame = new byte[0x1000];
                PopulateLzssFrame(frame);  // Same frame initialization as unpacker
                int framePos = 0xfee;      // Match unpacker's starting position
                int src = 0;

                // Since each output byte is repeated 3 times (RGB), we only need to process 1/3 of input
                while (src < m_input.Length / 3)
                {
                    byte control = 0;
                    int controlPos = (int)m_output.Position;
                    writer.Write(control);  // Placeholder for control byte

                    // Process 8 sequences (one bit per sequence in control byte)
                    for (int bit = 0; bit < 8 && src < m_input.Length / 3; bit++)
                    {
                        // For this implementation, we'll just use literal bytes
                        // Set the corresponding bit in control byte to indicate literal
                        control |= (byte)(1 << bit);
                
                        // Take one byte from each RGB triplet as our grayscale value
                        byte value = m_input[src * 3]; // Take R value (could also use G or B since they're same)
                        writer.Write(value);
                
                        // Update the frame buffer just like the unpacker does
                        frame[framePos++] = value;
                        framePos &= 0xfff;
                
                        // Move to next grayscale value (skip 3 bytes since they're identical)
                        src++;
                    }

                    // Go back and update control byte
                    long currentPos = m_output.Position;
                    m_output.Position = controlPos;
                    writer.Write(control);
                    m_output.Position = currentPos;
                }
            }
    
            return m_output.ToArray();
        }
        public byte[] PackV2()
        {
            // This is a simplified V2 packer that uses basic prediction
            using (var writer = new BinaryWriter(m_output))
            {
                // Write first pixel directly
                writer.Write(m_input[0]);
                writer.Write(m_input[1]);
                writer.Write(m_input[2]);

                
                int src = 3;
                int stride = m_stride;

                // For first row, use previous pixel prediction
                for (int x = 1; x < m_width; x++)
                {
                    // Signal that we're writing raw bytes (00 in two bits)
                    writer.Write((byte)0); // control bits
                    writer.Write(m_input[src++]);
                    writer.Write(m_input[src++]);
                    writer.Write(m_input[src++]);
                }

                // For remaining rows, use upper pixel prediction
                for (int y = 1; y < m_height; y++)
                {
                    for (int x = 0; x < m_width; x++)
                    {
                        // Signal raw bytes
                        writer.Write((byte)0);
                        writer.Write(m_input[src++]);
                        writer.Write(m_input[src++]);
                        writer.Write(m_input[src++]);
                    }
                }
            }

            return m_output.ToArray();
        }

        public byte[] PackV3(byte[] alpha)
        {
            // Pack RGB data using V2
            byte[] rgbData = PackV2();
            
            // Pack alpha channel using V0
            var alphaPacker = new Packer(alpha, m_width, m_height, 0);
            byte[] alphaData = alphaPacker.PackV0();
            
            // Combine RGB and alpha data
            using (var writer = new BinaryWriter(m_output))
            {
                writer.Write(rgbData);
                writer.Write(alphaData.Length);
                writer.Write(alphaData);
            }
            
            return m_output.ToArray();
        }
        
        void PopulateLzssFrame(byte[] frame)
        {
            int fill = 0;
            int ecx;
            for (int al = 0; al < 0x100; ++al)
            for (ecx = 0x0d; ecx > 0; --ecx)
                frame[fill++] = (byte)al;
            for (int al = 0; al < 0x100; ++al)
                frame[fill++] = (byte)al;
            for (int al = 0xff; al >= 0; --al)
                frame[fill++] = (byte)al;
            for (ecx = 0x80; ecx > 0; --ecx)
                frame[fill++] = 0;
            for (ecx = 0x6e; ecx > 0; --ecx)
                frame[fill++] = 0x20;
            for (ecx = 0x12; ecx > 0; --ecx)
                frame[fill++] = 0;
        }
    }
}