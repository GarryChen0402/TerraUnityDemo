using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    private TMP_Text coinText;

    private void Awake()
    {
        coinText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (PlayerCurrency.Instance != null)
            coinText.text = PlayerCurrency.Instance.FormatCurrency();
    }
}
