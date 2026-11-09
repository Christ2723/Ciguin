using UnityEngine;

//Bandera al final del nivel: al tocarla se completa el nivel
public class Meta : MonoBehaviour {

	public Transform bandera;
	public float alturaFinal = 1.5f;
	bool tocada;

	void OnTriggerEnter2D(Collider2D otro) {
		if (tocada || otro.GetComponent<Jugador>() == null) return;
		tocada = true;
		//Más puntos cuanto más alto toque el poste
		float altura = otro.transform.position.y - transform.position.y;
		int puntos = altura > 6f ? 5000 : altura > 4f ? 2000 : altura > 2f ? 800 : 400;
		Nivel.I.PuntosFlotantes(puntos, otro.transform.position);
		Nivel.I.Completar();
	}

	void Update() {
		if (tocada && bandera.localPosition.y > alturaFinal) bandera.localPosition += Vector3.down * 6f * Time.deltaTime;
	}
}
