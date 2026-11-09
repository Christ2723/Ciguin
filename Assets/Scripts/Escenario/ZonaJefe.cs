using UnityEngine;

//Al entrar a la arena se cierra la pared detrás de Ciguín, cambia la música y despierta el jefe
public class ZonaJefe : MonoBehaviour {

	public Jefe jefe;
	public GameObject muro;
	public float camaraMinX;
	bool activada;

	void OnTriggerEnter2D(Collider2D otro) {
		if (activada || otro.GetComponent<Jugador>() == null) return;
		activada = true;
		muro.SetActive(true);
		CamaraSeguir.I.minX = camaraMinX;
		Juego.I.Sonar(Juego.I.sTemblor);
		CamaraSeguir.I.Sacudir(0.4f, 0.2f);
		jefe.Despertar();
	}
}
