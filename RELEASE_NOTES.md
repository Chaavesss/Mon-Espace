Mon espace 1.8.1 — coordonnées cliquables dans le compagnon Dofus.

Les positions entre crochets, par exemple `[-3, 12]`, deviennent des liens dans les instructions et les en-têtes du compagnon et de son overlay. Un clic copie `/travel -3,12` et affiche une coche temporaire. Les coordonnées restent lisibles et plusieurs positions dans une même instruction sont copiées indépendamment.

La copie ne change ni la quête ni l’instruction courante. Le mode discret et les aperçus bloquent la copie. Le suivi Tougli et les instructions Ganymède existantes sont conservés.

Validation : 12 suites Windows natives, tests de clic sur les liens (commande et retour visuel), coordonnées multiples, signes, format invalide, copie bloquée et presse-papiers occupé. PDF natif non retesté sur cette version, comme déclaré dans le rapport CI.
