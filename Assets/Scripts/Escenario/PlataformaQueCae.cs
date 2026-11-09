using System.Collections;
using UnityEngine;

//Obstáculo 5 - Plataforma que se cae: tiembla cuando Ciguín la pisa, cae y reaparece después
public class PlataformaQueCae : MonoBehaviour {

	public float espera = 0.45f;
	public float reaparecer = 4f;
	public Transform grafico;
	Collider2D col;
	Vector3 inicio;
	bool activa;

	void Awake() {
		col = GetComponent<Collider2D>();
		inicio = transform.position;
	}

	void Update() {
		if (!activa && Jugador.I.SueloActual == col) StartCoroutine(Caer());
	}

	IEnumerator Caer() {
		activa = true;
		for (float t = 0f; t < espera; t += Time.deltaTime) {
			grafico.localPosition = new Vector3(Mathf.Sin(t * 80f) * 0.05f, 0f, 0f);
			yield return null;
		}
		grafico.localPosition = Vector3.zero;
		Juego.I.Sonar(Juego.I.sCaida, 0.4f);
		float v = 0f;
		for (float t = 0f; t < 1.6f; t += Time.deltaTime) {
			v += 25f * Time.deltaTime;
			transform.position += Vector3.down * v * Time.deltaTime;
			if (t > 0.25f) col.enabled = false;
			yield return null;
		}
		grafico.gameObject.SetActive(false);
		yield return new WaitForSeconds(reaparecer);
		transform.position = inicio;
		grafico.gameObject.SetActive(true);
		col.enabled = true;
		activa = false;
	}
}
