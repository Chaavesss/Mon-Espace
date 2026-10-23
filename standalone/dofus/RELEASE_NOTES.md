# Compagnon Dofus 1.7.2 — Temps par combat

- Historique par combat : durée du combat uniquement, numéro de salle et résultat. Chaque tentative reste distincte.
- Le cumul des combats remplace le compteur de temps passé dans la salle. Marchés et pauses entre combats sont exclus.
- Le temps total de la run reste séparé et continue entre les combats.

# Compagnon Dofus 1.7.1 — Corriger la sélection du jeu pour le chrono

- Le compagnon et les autres outils ne sont plus proposés comme fenêtres DOFUS. Le filtre identifie le processus du jeu et exclut explicitement son propre processus.
- Le jeu sélectionné est revérifié à l’activation et avant chaque capture, y compris après fermeture ou remplacement d’une fenêtre.
- Une seule fenêtre peut être présélectionnée ; en multi-compte, choix explicite du personnage. Actualiser conserve un choix encore valide.
- Messages explicites lorsqu’aucun jeu n’est détecté ou que plusieurs fenêtres sont disponibles.
- Tests Windows : exclusion du compagnon visible, refus d’une sélection périmée par le vrai bouton d’activation, multi-compte et contrôles existants du chrono/OCR.

Après mise à jour : ouvrir DOFUS, actualiser la liste dans Songes, sélectionner le jeu, activer la détection puis revenir dans le jeu. Ne pas laisser l’overlay couvrir les repères du jeu.

# Compagnon Dofus 1.7.0 — Import de quêtes terminées

- Import local de captures PNG/JPG/BMP, sélection du personnage et lecture OCR française Windows.
- Texte corrigeable, propositions par noms exacts ou proches, vérification et confirmation avant écriture.
- Quêtes correspondantes partagées entre guides ; trajets, notes et pages composites Ganymède non cochés en bloc.
- Doublons ignorés, autres personnages préservés, 30 derniers imports annulables. Les captures ne sont ni conservées ni transmises.
- Catalogues déjà chargés nécessaires ; saisie/collage de noms possible sans OCR.

Validation : tests du modèle, de la vraie interface WPF et de la sauvegarde. OCR testé sur capture synthétique Windows ; captures réelles du journal du joueur non fournies pour cette validation. PDF natif non retesté.

# Compagnon Dofus 1.6.1

- Reprendre la dernière session de personnage, parcours et étape depuis la bibliothèque. Les profils et progressions existants restent conservés.
- Notes personnelles par personnage et étape, visibles dans l’overlay. Préparation cochable des ressources, prérequis et kamas explicitement indiqués par le guide ; ajouts personnels possibles. Aucun inventaire ni compte de jeu n’est lu.
- Réglages de l’overlay : tailles du portrait/Dofus, opacité et éléments visibles. Modifications enregistrées localement.
- Meilleurs temps Songes conservés séparément de la liste récente des runs : un ancien record reste disponible quand l’historique s’allonge.
- Songes : records officiels séparés des records mesurés, comparaison des temps par salle avec la meilleure run mesurée, signalement d’un nouveau record officiel. Les défaites et archives incomplètes ne deviennent pas des records.

Les archives de runs existantes sont réutilisées. Une run terminée reste dans son fichier courant ; Nouvelle run archive le bilan précédent. Les comparaisons par salle utilisent uniquement les profondeurs effectivement enregistrées, sans inventer les salles manquantes. Les archives illisibles sont ignorées et conservées.

Extraire tout le ZIP, fermer l’ancienne version puis lancer Installer_Compagnon_Dofus.cmd. Conserver les DLL avec l’exécutable. Les données existantes sont conservées. Cette version concerne uniquement le compagnon indépendant.

Validation : modèles et boutons réels des notes/options, persistance, reprise et comparaison Songes. Les captures de validation sont fictives. La lecture visuelle Songes reste expérimentale et doit être vérifiée sur votre interface de jeu.
