namespace HyperCasual.Runner
{
    /// <summary>
    /// An ID that the audio system knows audio clips by
    /// </summary>
    public enum SoundID
    {
        None,
        CoinSound,
        KeySound,
        ButtonSound,
        MenuMusic,
        CorrectSound,      // 정답 시 재생될 사운드
        IncorrectSound     // 오답 시 재생될 사운드
    }
}