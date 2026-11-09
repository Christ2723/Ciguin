using UnityEngine;

//Mueve un objeto hacia arriba y lo destruye (puntos flotantes) o lo hace oscilar (decoración)
public class Flotar : MonoBehaviour {

	public float amplitud = 0.15f;
	public float frecuencia = 2f;
	float subir;
	float duracion;
	float t;
	Vector3 inicio;

	void Start() {
		inicio = transform.localPosition;
		t = transform.position.x;
	}

	public void Subir(float distancia, float segundos) {
		subir = distancia;
		duracion = segundos;
		amplitud = 0f;
	}

	void Update() {
		if (duracion > 0f) {
			transform.position += Vector3.up * (subir / duracion) * Time.deltaTime;
			duracion -= Time.deltaTime;
			if (duracion <= 0f) Destroy(gameObject);
			return;
		}
		t += Time.deltaTime;
		transform.localPosition = inicio + Vector3.up * Mathf.Sin(t * frecuencia) * amplitud;
	}
}
