using UnityEngine;

//Larimar: la piedra azul dominicana, es la "moneda" del juego. 50 larimares = 1 vida extra.
public class Larimar : MonoBehaviour {

	public GameObject efectoBrillo;
	float fase;

	void Start() {
		fase = transform.position.x * 0.7f;
	}

	void Update() {
		//Gira (se aplasta en X) y flota un poco
		float s = Mathf.Cos(Time.time * 3.5f + fase);
		transform.localScale = new Vector3(Mathf.Max(Mathf.Abs(s), 0.15f), 1f, 1f);
	}

	void OnTriggerEnter2D(Collider2D otro) {
		if (otro.GetComponent<Jugador>() == null) return;
		Juego.I.AgregarLarimar();
		if (efectoBrillo != null) Instantiate(efectoBrillo, transform.position, Quaternion.identity);
		Destroy(gameObject);
	}
}
