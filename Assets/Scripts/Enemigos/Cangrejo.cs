using UnityEngine;

//Enemigo 1 - Cangrejo: camina de lado y se da la vuelta en las paredes y en los bordes
public class Cangrejo : Enemigo {

	public float velocidad = 2f;
	public bool girarEnBordes = true;
	public LayerMask mascaraSuelo;
	float direccion = -1f;
	float ultimoGiro;

	void FixedUpdate() {
		if (!vivo) return;
		if (!Activo) {
			rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
			return;
		}

		Vector2 pos = rb.position;
		bool pared = Physics2D.Raycast(pos, new Vector2(direccion, 0f), 0.55f, mascaraSuelo);
		bool hayPiso = Physics2D.Raycast(pos + new Vector2(direccion * 0.5f, 0f), Vector2.down, 0.9f, mascaraSuelo);
		bool enPiso = Physics2D.Raycast(pos, Vector2.down, 0.6f, mascaraSuelo);
		if ((pared || (girarEnBordes && enPiso && !hayPiso)) && Time.time - ultimoGiro > 0.2f) {
			direccion = -direccion;
			ultimoGiro = Time.time;
		}
		rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
	}
}
