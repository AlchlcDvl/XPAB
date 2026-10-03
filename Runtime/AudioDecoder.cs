using System.Text;
using NVorbis;

namespace XPAB.Reader;

internal static class AudioDecoder
{
    public static void DecodeVorbis(Stream dataStream, float[] audioData, int totalSamples)
    {
        using var vorbis = new VorbisReader(dataStream, false);
        vorbis.ReadSamples(audioData, 0, totalSamples);
    }

    public static void DecodeAdpcm(Stream dataStream, float[] audioData, int channels)
    {
        var pcm = new short[audioData.Length];
        using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);

        for (var ch = 0; ch < channels; ch++)
        {
            var predictedSample = 0;
            var stepIndex = 0;

            var highNibble = true;
            byte currentByte = 0;

            for (var i = ch; i < pcm.Length; i += channels)
            {
                int code;

                if (highNibble)
                {
                    currentByte = reader.ReadByte();
                    code = (currentByte >> 4) & 0x0F;
                    highNibble = false;
                }
                else
                {
                    code = currentByte & 0x0F;
                    highNibble = true;
                }

                var step = AudioCodecInfo.AdpcmStepSizeTable[stepIndex];
                var delta = step >> 3;

                if ((code & 4) != 0)
                    delta += step;

                if ((code & 2) != 0)
                    delta += step >> 1;

                if ((code & 1) != 0)
                    delta += step >> 2;

                if ((code & 8) != 0)
                    predictedSample -= delta;
                else
                    predictedSample += delta;

                predictedSample = Math.Clamp(predictedSample, -32768, 32767);

                stepIndex += AudioCodecInfo.AdpcmIndexTable[code & 7];
                stepIndex = Math.Clamp(stepIndex, 0, 88);

                pcm[i] = (short)predictedSample;
            }
        }

        for (var i = 0; i < pcm.Length; i++)
            audioData[i] = pcm[i] / 32767f;
    }

    public static void DequantizeAudio(float[] audioData, AudioBitDepth bitDepth, XPABReader fileReader)
    {
        switch (bitDepth)
        {
            case AudioBitDepth.Int8:
            {
                for (var i = 0; i < audioData.Length; i++)
                    audioData[i] = fileReader.ReadSByte() / 127f;
                break;
            }

            case AudioBitDepth.Int16:
            {
                for (var i = 0; i < audioData.Length; i++)
                    audioData[i] = fileReader.ReadInt16() / 32767f;
                break;
            }

            default:
            {
                for (var i = 0; i < audioData.Length; i++)
                    audioData[i] = fileReader.ReadSingle();
                break;
            }
        }
    }
}
