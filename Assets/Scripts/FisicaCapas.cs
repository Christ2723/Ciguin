using UnityEngine;

//Qué capas chocan entre sí. Se aplica al iniciar el juego.
// Suelo: con todo | Jugador: Suelo, Golpe, Recogible | Enemigos: Suelo
// Golpe (zonas de daño, proyectiles): Jugador, Suelo | Objetos (mango): Suelo | Recogible: Jugador
public static class FisicaCapas {

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Configurar() {
		string[] capas = { "Suelo", "Jugador", "Enemigos", "Golpe", "Objetos", "Recogible" };
		foreach (var a in capas) {
			foreach (var b in capas) {
				int la = LayerMask.NameToLayer(a), lb = LayerMask.NameToLayer(b);
				if (la < 0 || lb < 0) continue;
				Physics2D.IgnoreLayerCollision(la, lb, !Chocan(a, b));
			}
		}
	}

	static bool Chocan(string a, string b) {
		if (a == "Suelo" || b == "Suelo") return true;
		bool Par(string x, string y) => (a == x && b == y) || (a == y && b == x);
		return Par("Jugador", "Golpe") || Par("Jugador", "Recogible");
	}
}
