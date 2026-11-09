using UnityEngine;

//Trigger hijo de un enemigo: avisa al enemigo cuando el jugador lo toca
public class ZonaGolpe : MonoBehaviour {

	public Enemigo enemigo;
	Collider2D zona;

	void Awake() {
		zona = GetComponent<Collider2D>();
	}

	void OnTriggerEnter2D(Collider2D otro) => Revisar(otro);
	void OnTriggerStay2D(Collider2D otro) => Revisar(otro);

	void Revisar(Collider2D otro) {
		var j = otro.GetComponent<Jugador>();
		if (j != null && !j.Muerto) enemigo.TocarJugador(j, zona);
	}
}
