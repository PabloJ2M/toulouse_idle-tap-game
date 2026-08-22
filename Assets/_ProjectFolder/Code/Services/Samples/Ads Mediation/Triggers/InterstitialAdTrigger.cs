namespace Unity.Services.LevelPlay
{
    public class InterstitialAdTrigger : AdTriggerBehaviour
    {
        public override void TriggerAd() => InterstitialAd.Instance.TryDisplayAd();
    }
}