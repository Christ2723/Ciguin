using UnityEngine;

//Obstáculo que hace daño al tocarlo (púas, cocos, estalactitas, espinas, bolas de fuego)
public class Peligro : MonoBehaviour {

	public string motivo = "Ciguín se lastimó con un obstáculo.";

	void OnTriggerEnter2D(Collider2D otro) => Revisar(otro);
	void OnTriggerStay2D(Collider2D otro) => Revisar(otro);

	void Revisar(Collider2D otro) {
		var j = otro.GetComponent<Jugador>();
		if (j != null && !j.Muerto && enabled) j.Danar(transform.position, motivo);
	}
}
