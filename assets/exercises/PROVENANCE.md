# Free Exercise DB

- Source : https://github.com/yuhonas/free-exercise-db
- Révision figée : `f00c92c7dcf1216a928a52c3706c7ce8e2f71ed5`.
- Catalogue : `dist/exercises.json`, 876 fiches ; espaces JSON compactés sans suppression de champs.
- Photos : chemins `exercises/<id>/0.jpg` et `1.jpg`, 1 746 fichiers. Toutes les photos référencées sont incluses.
- Licence déclarée par la source : Unlicense, texte conservé dans `LICENSE.txt` et embarqué dans le programme.
- `photos-manifest.json` conserve les chemins, tailles et SHA-256 des fichiers importés.

Les champs d'origine sont conservés. Mon Espace traduit les catégories, muscles, niveaux et matériels ; les fiches françaises déjà créées remplacent le nom et les consignes de leurs correspondances, tout en conservant le texte anglais consultable. `fr.json` contient les noms et consignes en français pour les 876 identifiants source : traduction automatique Google Translate réalisée lors de l'import, non révisée individuellement. Les cinq fiches sans consignes source (`Iron_Cross`, `Push_Press`, `Side_Bridge`, `Side_Jackknife`, `One-Arm_Kettlebell_Swings`) ont un nom français et une indication d'absence de consignes. Le catalogue source contient aussi du cardio, des étirements et des variantes de mouvements.

Les données et photos sont chargées depuis les ressources embarquées. Les photos sont décodées à la demande pour les 24 résultats de la page et la fiche ouverte. Aucune requête distante n'est faite pendant la consultation.

Pour reconstruire : `./scripts/prepare-exercise-assets.ps1` (également appelé par `build.ps1`). Les fichiers téléchargés sont vérifiés par rapport au manifeste avant compilation.
