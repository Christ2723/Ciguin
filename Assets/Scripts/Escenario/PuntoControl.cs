using UnityEngine;

//Bandera de punto de control: si Ciguín cae al vacío, reaparece aquí
public class PuntoControl : MonoBehaviour {

	public SpriteRenderer render;
	public Sprite spriteActivo;
	bool activo;

	void OnTriggerEnter2D(Collider2D otro) {
		if (activo || otro.GetComponent<Jugador>() == null) return;
		activo = true;
		render.sprite = spriteActivo;
		Juego.I.Sonar(Juego.I.sCheckpoint);
		Nivel.I.PuntoDeControl(transform.position);
		Nivel.I.MostrarMensaje("PUNTO DE CONTROL", "", 1.2f);
	}
}
