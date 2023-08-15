using System.Collections;
using System.Collections.Generic;
using CustomAttributes;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteToggle : MonoBehaviour
{
    [SerializeField, Required]
    private Sprite _sprite1;

    [SerializeField, Required]
    private Sprite _sprite2;

    [SerializeField, Required]
    private Image _image;

    private bool _isSprite1 = true;

    private void Awake() {
        // Always starts with sprite 1
        _image.sprite = _sprite1;
        _isSprite1 = true;
    }

    /// <summary>
    /// Toggle the sprite between sprite 1 and sprite 2
    /// </summary>
    public void Toggle() {
        if (_isSprite1) {
            _image.sprite = _sprite2;
            _isSprite1 = false;
        } else {
            _image.sprite = _sprite1;
            _isSprite1 = true;
        }
    }
}
