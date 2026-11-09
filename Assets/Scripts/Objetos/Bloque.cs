using System.Collections;
using UnityEngine;

//Bloques que Ciguín golpea con la cabeza:
//- Sorpresa: suelta un larimar, varios larimares o un mango y queda vacío.
//- Ladrillo: se rompe en pedazos.
//- Vacio: solo suena.
public class Bloque : MonoBehaviour {

	public enum Tipo { Sorpresa, Ladrillo, Vacio }
	public enum Contenido { Larimar, VariosLarimares, Mango }

	public Tipo tipo = Tipo.Sorpresa;
	public Contenido contenido = Contenido.Larimar;
	public int larimaresRestantes = 6;
	public Transform grafico;
	public SpriteRenderer render;
	public Sprite[] framesSorpresa;
	public Sprite spriteVacio;
	public GameObject prefabLarimarSaltando;
	public GameObject prefabMango;
	public GameObject prefabEscombro;

	bool animando;

	void Update() {
		if (tipo == Tipo.Sorpresa && framesSorpresa.Length > 0) {
			render.sprite = framesSorpresa[(int)(Time.time * 4f) % framesSorpresa.Length];
		}
	}

	public void Golpear() {
		if (animando) return;
		DerribarEnemigosEncima();

		switch (tipo) {
			case Tipo.Sorpresa:
				if (contenido == Contenido.Mango) {
					Instantiate(prefabMango, transform.position, Quaternion.identity);
					Juego.I.Sonar(Juego.I.sAparece);
					Vaciar();
				} else {
					Instantiate(prefabLarimarSaltando, transform.position + Vector3.up, Quaternion.identity);
					Juego.I.AgregarLarimar();
					if (contenido == Contenido.Larimar || --larimaresRestantes <= 0) Vaciar();
				}
				StartCoroutine(Rebote());
				break;
			case Tipo.Ladrillo:
				Romper();
				break;
			default:
				Juego.I.Sonar(Juego.I.sBloque, 0.6f);
				StartCoroutine(Rebote());
				break;
		}
	}

	void Vaciar() {
		tipo = Tipo.Vacio;
		render.sprite = spriteVacio;
	}

	void Romper() {
		Juego.I.Sonar(Juego.I.sRomper);
		Nivel.I.PuntosFlotantes(50, transform.position);
		for (int i = 0; i < 4; i++) {
			var e = Instantiate(prefabEscombro, transform.position + new Vector3(i % 2 == 0 ? -0.25f : 0.25f, i < 2 ? 0.25f : -0.25f), Quaternion.identity);
			var rb = e.GetComponent<Rigidbody2D>();
			rb.linearVelocity = new Vector2(i % 2 == 0 ? -3f : 3f, i < 2 ? 12f : 8f);
			rb.angularVelocity = Random.Range(-720f, 720f);
			Destroy(e, 1.5f);
		}
		Destroy(gameObject);
	}

	void DerribarEnemigosEncima() {
		var encima = Physics2D.OverlapBoxAll((Vector2)transform.position + Vector2.up * 0.7f, new Vector2(0.9f, 0.5f), 0f,
			LayerMask.GetMask("Enemigos"));
		foreach (var c in encima) {
			var e = c.GetComponentInParent<Enemigo>();
			if (e != null && !(e is Jefe)) e.Derribar();
		}
	}

	IEnumerator Rebote() {
		animando = true;
		for (float t = 0f; t < 0.18f; t += Time.deltaTime) {
			grafico.localPosition = Vector3.up * Mathf.Sin(t / 0.18f * Mathf.PI) * 0.35f;
			yield return null;
		}
		grafico.localPosition = Vector3.zero;
		animando = false;
	}
}
