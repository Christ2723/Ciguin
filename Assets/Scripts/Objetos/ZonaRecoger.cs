using UnityEngine;

//Trigger hijo del mango que detecta a Ciguín
public class ZonaRecoger : MonoBehaviour {

	public Mango mango;

	void OnTriggerStay2D(Collider2D otro) {
		if (otro.GetComponent<Jugador>() != null) mango.Recoger();
	}
}
