# Compagnon Dofus 1.9.2 — Fluidité de l’overlay

- Navigation entre destinations : mise à jour de la carte et des boutons sans reconstruction du reste de la fenêtre.
- Liste complète des destinations construite seulement à son ouverture.
- Chargement des instructions des guides en arrière-plan, avec une seule lecture simultanée et rejet des résultats devenus obsolètes.
- Correspondance des quêtes par index en mémoire, au lieu de reparcourir et normaliser tous les extraits à chaque affichage.
- Réutilisation de l’itinéraire analysé et des portraits décodés ; invalidation du portrait si son fichier change.
- Texte et dimensions de la 1.9.1 conservés.
- Tests de performance : 20 000 extraits, chargement volontairement bloqué avec vérification de réponse WPF, et 20 clics réels sur un itinéraire de 300 destinations.
