using UnityEngine;

//Animación corta que se destruye al terminar (brillos, polvo, explosiones, larimar que salta)
public class Efecto : MonoBehaviour {

	public SpriteRenderer render;
	public Sprite[] frames;
	public float fps = 14f;
	public Vector2 velocidad;
	public float gravedad;
	float t;

	void Update() {
		t += Time.deltaTime;
		velocidad.y -= gravedad * Time.deltaTime;
		transform.position += (Vector3)velocidad * Time.deltaTime;
		int f = (int)(t * fps);
		if (f >= frames.Length) {
			Destroy(gameObject);
			return;
		}
		render.sprite = frames[f];
	}
}
