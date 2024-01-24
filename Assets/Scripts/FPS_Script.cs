using System;
using TMPro;
using UnityEngine;

public class FPS_Script:MonoBehaviour
{
        private TextMeshProUGUI _text;

        // A little script to show fps
        
        private void Awake()
        {
                _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
                _text.text = Mathf.RoundToInt(1 / Time.smoothDeltaTime).ToString();
        }
}