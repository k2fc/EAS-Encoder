public class SAMEAudioBit
{
    public int frequency;
    public decimal length;
    public decimal level;

    public SAMEAudioBit(int Frequency, decimal Length)
    {
        frequency = Frequency;
        length = Length;
        level = 1.0M;
    }

    public SAMEAudioBit(int Frequency, decimal Length, decimal Level)
    {
        frequency = Frequency;
        length = Length;
        level = Level;
    }
}