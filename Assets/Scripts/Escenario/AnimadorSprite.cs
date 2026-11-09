using UnityEngine;
using UnityEngine.UI;

//Anima un SpriteRenderer o una Image de la interfaz con una lista de sprites
public class AnimadorSprite : MonoBehaviour {

	public Sprite[] frames;
	public float fps = 6f;
	public bool desfaseAleatorio = true;
	SpriteRenderer sr;
	Image img;
	float desfase;

	void Awake() {
		sr = GetComponent<SpriteRenderer>();
		img = GetComponent<Image>();
		if (desfaseAleatorio) desfase = Random.Range(0f, 10f);
	}

	void Update() {
		if (frames == null || frames.Length == 0) return;
		var s = frames[(int)((Time.unscaledTime + desfase) * fps) % frames.Length];
		if (sr != null) sr.sprite = s;
		if (img != null) img.sprite = s;
	}
}
