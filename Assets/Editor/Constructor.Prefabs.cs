using UnityEditor;
using UnityEngine;

//Prefabs: Ciguín, enemigos, jefe, bloques, objetos, plataformas y efectos
public static partial class ConstructorProyecto {

	static GameObject pJugador, pCangrejo, pMurcielago, pCactus, pCoco, pEstalactita, pPiedraCae, pPuas, pJefe;
	static GameObject pBloque, pLarimar, pMango, pEscombro, pLarimarSalta, pBrillo, pPolvo, pExplosion;
	static GameObject pPlataforma, pPlataformaMovil, pPlataformaCae, pMeta, pPuntoControl;
	static Proyectil pEspina, pFuego, pPiedra;

	static GameObject CrearEfectoPrefab(string nombre, Sprite[] frames, float fps, int orden, Vector2 vel = default, float gravedad = 0f) {
		var go = Objeto(nombre);
		var e = go.AddComponent<Efecto>();
		e.render = Render(go, frames[0], orden);
		e.frames = frames;
		e.fps = fps;
		e.velocidad = vel;
		e.gravedad = gravedad;
		return Guardar(go, nombre);
	}

	static void CrearPrefabs() {

		int suelo = 1 << CAPA_SUELO;

		// ---- Efectos
		pBrillo = CrearEfectoPrefab("Efecto_Brillo", sBrillo, 16f, 30);
		pPolvo = CrearEfectoPrefab("Efecto_Polvo", sPolvo, 12f, 30);
		pExplosion = CrearEfectoPrefab("Efecto_Explosion", sExplosion, 12f, 45);
		var gira = new[] { sLarimarGira[0], sLarimarGira[1], sLarimarGira[2], sLarimarGira[3], sLarimarGira[0], sLarimarGira[1], sLarimarGira[2], sLarimarGira[3] };
		pLarimarSalta = CrearEfectoPrefab("Efecto_LarimarSalta", gira, 16f, 30, new Vector2(0f, 9f), 26f);

		var esc = Objeto("Escombro");
		Render(esc, sEscombro, 30);
		Cuerpo(esc, RigidbodyType2D.Dynamic, 3f).constraints = RigidbodyConstraints2D.None;
		pEscombro = Guardar(esc, "Escombro");

		// ---- Proyectiles
		Proyectil CrearProyectil(string nombre, Sprite s, float radio, string motivo, float giro, Sprite[] anim = null) {
			var go = Objeto(nombre, null, CAPA_GOLPE);
			var sr = Render(go, s, 25);
			if (anim != null) {
				var a = go.AddComponent<AnimadorSprite>();
				a.frames = anim;
				a.fps = 12f;
			}
			var rb = Cuerpo(go, RigidbodyType2D.Dynamic, 1.5f);
			rb.constraints = RigidbodyConstraints2D.None;
			Caja<CircleCollider2D>(go, new Vector2(radio, 0f), Vector2.zero, true);
			go.AddComponent<Peligro>().motivo = motivo;
			var p = go.AddComponent<Proyectil>();
			p.giro = giro;
			p.efectoImpacto = pPolvo;
			return Guardar(go, nombre).GetComponent<Proyectil>();
		}
		pEspina = CrearProyectil("Espina", sEspina, 0.17f, "Una espina de cactus pinchó a Ciguín.", 540f);
		pFuego = CrearProyectil("BolaDeFuego", sFuego[0], 0.32f, "Una bola de fuego del Galipote quemó a Ciguín.", 0f, sFuego);
		pPiedra = CrearProyectil("PiedraJefe", sPiedra, 0.38f, "Una piedra cayó sobre Ciguín.", 200f);

		// ---- Ciguín
		var jug = Objeto("Ciguin", null, CAPA_JUGADOR);
		var rbJ = Cuerpo(jug, RigidbodyType2D.Dynamic, 4f);
		var colJ = Caja<BoxCollider2D>(jug, new Vector2(0.56f, 0.78f), new Vector2(0f, -0.07f), false);
		((BoxCollider2D)colJ).edgeRadius = 0.04f;
		var j = jug.AddComponent<Jugador>();
		j.grafico = Grafico(jug, sJugadorQuieto[0], 10);
		j.quieto = sJugadorQuieto;
		j.caminar = sJugadorCaminar;
		j.saltar = sJugadorSaltar;
		j.caer = sJugadorCaer;
		j.herido = sJugadorHerido;
		j.celebrar = sJugadorCelebrar;
		j.mascaraSuelo = suelo;
		pJugador = Guardar(jug, "Ciguin");

		// ---- Enemigo 1: Cangrejo
		var can = Objeto("Cangrejo", null, CAPA_ENEMIGOS);
		Cuerpo(can, RigidbodyType2D.Dynamic, 3f);
		Caja<BoxCollider2D>(can, new Vector2(0.8f, 0.55f), new Vector2(0f, -0.08f), false);
		var c = can.AddComponent<Cangrejo>();
		c.grafico = Grafico(can, sCangrejo[0], 6);
		c.frames = sCangrejo;
		c.fpsAnimacion = 5f;
		c.mascaraSuelo = suelo;
		c.puntos = 200;
		c.motivo = "Un cangrejo pellizcó a Ciguín.";
		Zona(can, c, new Vector2(0.86f, 0.62f), new Vector2(0f, -0.05f));
		pCangrejo = Guardar(can, "Cangrejo");

		// ---- Enemigo 2: Murciélago
		var mur = Objeto("Murcielago", null, CAPA_ENEMIGOS);
		Cuerpo(mur, RigidbodyType2D.Kinematic, 0f);
		Caja<CircleCollider2D>(mur, new Vector2(0.3f, 0f), Vector2.zero, true);
		var m = mur.AddComponent<Murcielago>();
		m.grafico = Grafico(mur, sMurcielago[0], 6);
		m.frames = sMurcielago;
		m.fpsAnimacion = 8f;
		m.puntos = 300;
		m.motivo = "Un murciélago mordió a Ciguín.";
		Zona(mur, m, new Vector2(0.8f, 0.5f), Vector2.zero);
		pMurcielago = Guardar(mur, "Murcielago");

		// ---- Enemigo 3: Cactus lanzador (no se puede aplastar)
		var cac = Objeto("CactusLanzador", null, CAPA_ENEMIGOS);
		var ca = cac.AddComponent<CactusLanzador>();
		ca.grafico = Grafico(cac, sCactus[0], 6);
		ca.frames = sCactus;
		ca.spriteDisparo = sCactusDisparo;
		ca.prefabEspina = pEspina;
		ca.aplastable = false;
		ca.motivo = "Ciguín se pinchó con un cactus.";
		Zona(cac, ca, new Vector2(0.75f, 0.85f), new Vector2(0f, -0.05f));
		pCactus = Guardar(cac, "CactusLanzador");

		// ---- Obstáculo 4: objetos que caen (coco, estalactita)
		GameObject CrearQueCae(string nombre, Sprite s, Vector2 tamano, string motivo) {
			var go = Objeto(nombre, null, CAPA_GOLPE);
			Render(go, s, 7);
			var rb = Cuerpo(go, RigidbodyType2D.Kinematic, 0f);
			rb.useFullKinematicContacts = true;
			Caja<BoxCollider2D>(go, tamano, Vector2.zero, true);
			go.AddComponent<Peligro>().motivo = motivo;
			go.AddComponent<ObjetoQueCae>().efectoImpacto = pPolvo;
			return Guardar(go, nombre);
		}
		pCoco = CrearQueCae("Coco", sCoco, new Vector2(0.6f, 0.6f), "Un coco le cayó en la cabeza a Ciguín.");
		pEstalactita = CrearQueCae("Estalactita", sEstalactita, new Vector2(0.45f, 0.9f), "Una estalactita cayó sobre Ciguín.");
		pPiedraCae = CrearQueCae("PiedraCae", sPiedra, new Vector2(0.7f, 0.7f), "Una roca cayó sobre Ciguín.");

		// ---- Obstáculo 5: púas
		var pu = Objeto("Puas", null, CAPA_GOLPE);
		Render(pu, sPuas, 4);
		Caja<BoxCollider2D>(pu, new Vector2(0.8f, 0.4f), new Vector2(0f, -0.28f), true);
		pu.AddComponent<Peligro>().motivo = "Ciguín cayó sobre unas púas.";
		pPuas = Guardar(pu, "Puas");

		// ---- Jefe: El Galipote
		var jefe = Objeto("Galipote", null, CAPA_ENEMIGOS);
		var rbJefe = Cuerpo(jefe, RigidbodyType2D.Dynamic, 4f);
		rbJefe.mass = 50f;
		Caja<BoxCollider2D>(jefe, new Vector2(1.5f, 1.45f), new Vector2(0f, -0.25f), false);
		var g = jefe.AddComponent<Jefe>();
		g.grafico = Grafico(jefe, sJefe[0], 8);
		g.frames = sJefe;
		g.spriteAtaque = sJefeAtaque;
		g.spriteHerido = sJefeHerido;
		g.prefabFuego = pFuego;
		g.prefabPiedra = pPiedra;
		g.efectoExplosion = pExplosion;
		g.mascaraSuelo = suelo;
		g.puntos = 5000;
		g.motivo = "El Galipote derrotó a Ciguín.";
		Zona(jefe, g, new Vector2(1.7f, 1.6f), new Vector2(0f, -0.15f));
		jefe.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
		pJefe = Guardar(jefe, "Galipote");

		// ---- Bloque (sorpresa, ladrillo o vacío)
		var blo = Objeto("Bloque", null, CAPA_SUELO);
		Caja<BoxCollider2D>(blo, Vector2.one, Vector2.zero, false);
		var b = blo.AddComponent<Bloque>();
		b.render = Grafico(blo, sSorpresa[0], 2);
		b.grafico = b.render.transform;
		b.framesSorpresa = sSorpresa;
		b.spriteVacio = sVacio;
		b.prefabLarimarSaltando = pLarimarSalta;
		b.prefabEscombro = pEscombro;
		pBloque = blo; //se completa después de crear el mango

		// ---- Larimar
		var lar = Objeto("Larimar", null, CAPA_RECOGIBLE);
		Render(lar, sLarimar, 3);
		Caja<CircleCollider2D>(lar, new Vector2(0.35f, 0f), Vector2.zero, true);
		lar.AddComponent<Larimar>().efectoBrillo = pBrillo;
		pLarimar = Guardar(lar, "Larimar");

		// ---- Mango
		var man = Objeto("Mango", null, CAPA_OBJETOS);
		Render(man, sMango, 3);
		Cuerpo(man, RigidbodyType2D.Dynamic, 3f);
		Caja<BoxCollider2D>(man, new Vector2(0.7f, 0.7f), Vector2.zero, false);
		var mg = man.AddComponent<Mango>();
		mg.mascaraSuelo = suelo;
		var zm = Objeto("Zona", man.transform, CAPA_RECOGIBLE);
		Caja<BoxCollider2D>(zm, new Vector2(0.85f, 0.85f), Vector2.zero, true);
		zm.AddComponent<ZonaRecoger>().mango = mg;
		pMango = Guardar(man, "Mango");

		b.prefabMango = pMango;
		pBloque = Guardar(blo, "Bloque");

		// ---- Plataformas
		var pla = Objeto("PlataformaMadera", null, CAPA_SUELO);
		Mosaico(pla, sPlataforma, new Vector2(3f, 0.5f), 1);
		var colP = Caja<BoxCollider2D>(pla, new Vector2(3f, 0.3f), new Vector2(0f, 0.06f), false);
		colP.usedByEffector = true;
		var ef = pla.AddComponent<PlatformEffector2D>();
		ef.useOneWay = true;
		ef.surfaceArc = 150f;
		pPlataforma = Guardar(pla, "PlataformaMadera");

		var plm = Objeto("PlataformaMovil", null, CAPA_SUELO);
		Mosaico(plm, sPlataforma, new Vector2(3f, 0.5f), 1);
		var colM = Caja<BoxCollider2D>(plm, new Vector2(3f, 0.3f), new Vector2(0f, 0.06f), false);
		colM.usedByEffector = true;
		var efM = plm.AddComponent<PlatformEffector2D>();
		efM.useOneWay = true;
		efM.surfaceArc = 150f;
		var rbM = plm.AddComponent<Rigidbody2D>();
		rbM.bodyType = RigidbodyType2D.Kinematic;
		plm.AddComponent<PlataformaMovil>();
		pPlataformaMovil = Guardar(plm, "PlataformaMovil");

		var plc = Objeto("PlataformaQueCae", null, CAPA_SUELO);
		Caja<BoxCollider2D>(plc, new Vector2(2f, 0.36f), new Vector2(0f, 0.05f), false);
		var grC = Objeto("Grafico", plc.transform, CAPA_SUELO);
		Mosaico(grC, sPlataformaCae, new Vector2(2f, 0.5f), 1);
		plc.AddComponent<PlataformaQueCae>().grafico = grC.transform;
		pPlataformaCae = Guardar(plc, "PlataformaQueCae");

		// ---- Meta (bandera dominicana) y punto de control
		var meta = Objeto("Meta", null, CAPA_RECOGIBLE);
		var poste = Objeto("Poste", meta.transform);
		Mosaico(poste, sPoste, new Vector2(1f, 9f), 2);
		poste.transform.localPosition = new Vector3(0f, 4.5f, 0f);
		var bola = Objeto("Bola", meta.transform);
		Render(bola, sLarimar, 3);
		bola.transform.localPosition = new Vector3(0f, 9.2f, 0f);
		var bandera = Objeto("Bandera", meta.transform);
		Render(bandera, sBanderaMeta, 2);
		bandera.transform.localPosition = new Vector3(0.05f, 8.3f, 0f);
		Caja<BoxCollider2D>(meta, new Vector2(0.5f, 9.5f), new Vector2(0f, 4.75f), true);
		var mt = meta.AddComponent<Meta>();
		mt.bandera = bandera.transform;
		mt.alturaFinal = 1.2f;
		pMeta = Guardar(meta, "Meta");

		var pc = Objeto("PuntoControl", null, CAPA_RECOGIBLE);
		var pcs = Render(pc, sPuntoControl, 2);
		Caja<BoxCollider2D>(pc, new Vector2(0.8f, 2f), new Vector2(0.3f, 1f), true);
		var pcc = pc.AddComponent<PuntoControl>();
		pcc.render = pcs;
		pcc.spriteActivo = sPuntoControlActivo;
		pPuntoControl = Guardar(pc, "PuntoControl");
	}
}
