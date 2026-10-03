using OggVorbisEncoder;
using XPAB.Writer;

namespace XPAB;

internal static class AudioEncoder
{
    private static readonly Action<XPABWriter, float> WriteFloat = (w, v) => w.Write(v);

    public static void EncodeVorbis(float[] audioData, int channels, int frequency, Stream dataStream)
    {
        var samplesPerChannel = audioData.Length / channels;
        var deinterleaved = new float[channels][];

        for (var c = 0; c < channels; c++)
        {
            deinterleaved[c] = new float[samplesPerChannel];

            for (var s = 0; s < samplesPerChannel; s++)
                deinterleaved[c][s] = audioData[(s * channels) + c];
        }

        var info = VorbisInfo.InitVariableBitRate(channels, frequency, 0.4f);
        var oggStream = new OggStream(new System.Random().Next());

        var comments = new Comments();
        comments.AddTag("ENCODER", "XPAB Toolchain");

        oggStream.PacketIn(HeaderPacketBuilder.BuildInfoPacket(info));
        oggStream.PacketIn(HeaderPacketBuilder.BuildCommentsPacket(comments));
        oggStream.PacketIn(HeaderPacketBuilder.BuildBooksPacket(info));
        FlushOggPages(oggStream, dataStream, true);

        var processingState = ProcessingState.Create(info);
        processingState.WriteData(deinterleaved, samplesPerChannel);
        processingState.WriteEndOfStream();

        while (!oggStream.Finished && processingState.PacketOut(out var packet))
        {
            oggStream.PacketIn(packet);
            FlushOggPages(oggStream, dataStream, false);
        }

        FlushOggPages(oggStream, dataStream, true);
    }

    private static void FlushOggPages(OggStream oggStream, Stream output, bool force)
    {
        while (oggStream.PageOut(out var page, force))
        {
            output.Write(page.Header, 0, page.Header.Length);
            output.Write(page.Body, 0, page.Body.Length);
        }
    }

    public static void EncodeAdpcm(float[] audioData, int channels, Stream dataStream)
    {
        var pcm = new short[audioData.Length];

        for (var i = 0; i < audioData.Length; i++)
            pcm[i] = (short)(Math.Clamp(audioData[i], -1f, 1f) * 32767f);

        using var writer = new BinaryWriter(dataStream, System.Text.Encoding.UTF8, true);

        for (var ch = 0; ch < channels; ch++)
        {
            var predictedSample = 0;
            var stepIndex = 0;

            var highNibble = true;
            byte currentByte = 0;

            for (var i = ch; i < pcm.Length; i += channels)
            {
                var sample = pcm[i];
                var diff = sample - predictedSample;
                var step = AudioCodecInfo.AdpcmStepSizeTable[stepIndex];

                var code = 0;

                if (diff < 0)
                {
                    code = 8;
                    diff = -diff;
                }

                var delta = step >> 3;

                if (diff >= step)
                {
                    code |= 4;
                    diff -= step;
                    delta += step;
                }

                step >>= 1;

                if (diff >= step)
                {
                    code |= 2;
                    diff -= step;
                    delta += step;
                }

                step >>= 1;

                if (diff >= step)
                {
                    code |= 1;
                    delta += step;
                }

                if ((code & 8) != 0)
                    predictedSample -= delta;
                else
                    predictedSample += delta;

                predictedSample = Math.Clamp(predictedSample, -32768, 32767);

                stepIndex += AudioCodecInfo.AdpcmIndexTable[code & 7];
                stepIndex = Math.Clamp(stepIndex, 0, 88);

                if (highNibble)
                {
                    currentByte = (byte)((code << 4) & 0xF0);
                    highNibble = false;
                }
                else
                {
                    currentByte |= (byte)(code & 0x0F);
                    writer.Write(currentByte);
                    highNibble = true;
                }
            }

            if (!highNibble)
                writer.Write(currentByte);
        }
    }

    public static void QuantizeAudio(float[] audioData, AudioBitDepth bitDepth, XPABWriter writer)
    {
        switch (bitDepth)
        {
            case AudioBitDepth.Int8:
            {
                for (var i = 0; i < audioData.Length; i++)
                    writer.Write((sbyte)(Math.Clamp(audioData[i], -1f, 1f) * 127f));

                break;
            }

            case AudioBitDepth.Int16:
            {
                for (var i = 0; i < audioData.Length; i++)
                    writer.Write((short)(Math.Clamp(audioData[i], -1f, 1f) * 32767f));

                break;
            }

            default:
            {
                writer.WriteArray(audioData, WriteFloat);
                break;
            }
        }
    }
}
