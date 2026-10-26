# Sport & Nutrition

Accès : **Sport & Nutrition** dans la navigation de Mon Espace. Le module reprend les couleurs, cartes et dialogues du thème actif.

## Musculation

### Bibliothèque d'exercices

L'onglet **Exercices** importe les **876 exercices et 1 746 photos** de Free Exercise DB, en plus des éventuels mouvements français sans correspondance dans cette source. Les mouvements déjà traduits sont fusionnés avec leurs fiches source pour conserver leurs noms et consignes en français. Recherchez un nom anglais ou français disponible (avec ou sans accents), puis combinez les filtres muscle, matériel, niveau et catégorie. L'affichage est paginé par 24 résultats. Chaque fiche décrit les muscles principaux et secondaires, les consignes et les photos disponibles.

Depuis une fiche, choisissez **Ajouter à une séance**. Depuis un modèle de séance, **+ Bibliothèque** utilise directement ce modèle. Les séries, répétitions, charge, repos et consignes restent modifiables avant enregistrement. Les valeurs préremplies sont des points de départ ; la charge initiale est zéro et doit être adaptée. L'historique reste indépendant des modifications. **+ Exercice personnel** permet de saisir un mouvement absent ; les mouvements personnels des séances sont aussi récapitulés dans la bibliothèque.

Les noms et consignes sont disponibles en français. Les nouvelles fiches utilisent une traduction automatique, signalée dans la fiche et non révisée individuellement ; le texte anglais d'origine reste consultable et recherchable. Les cinq fiches sans consignes dans la source le signalent explicitement. Les muscles, matériels, niveaux et catégories utilisent des libellés français contrôlés. Les photos montrent des positions : ce ne sont pas des vidéos ou animations. Les catégories comprennent aussi étirements et cardio ; les modèles de séances enregistrent actuellement des répétitions et charges, pas une durée ou distance par série.

Le catalogue et toutes les photos sont embarqués dans l'exécutable : aucune connexion n'est nécessaire pendant l'utilisation. Le volume des photos augmente l'exécutable d'environ 99 Mo. La compilation récupère les photos manquantes depuis une révision précise du dépôt, vérifie chaque empreinte SHA-256 et prépare leur archive. Le dossier photo généré est exclu de Git ; son manifeste est versionné. Source et licence : [provenance](../assets/exercises/PROVENANCE.md).

- Créer des modèles de séances et ajouter des exercices libres.
- Définir les séries, répétitions, charges en kg, repos en secondes et consignes.
- Lancer une séance ; ajuster et cocher chaque série individuellement.
- Utiliser **Repos** pour démarrer le minuteur. Son échéance est sauvegardée et calculée à partir de l'heure UTC, même si la fenêtre est fermée.
- Utiliser **Enregistrer & fermer** pour conserver un entraînement en cours. Fermer par la croix ou Échap laisse le dernier brouillon enregistré.
- Terminer après au moins une série réalisée. Une seule séance peut être en cours.
- Consulter les charges, répétitions, volume des séries cochées et historique dans **Progression**. Les séries non cochées sont exclues.
- Modifier ou supprimer un modèle conserve l'historique des séances réalisées. Le volume = somme des répétitions × kg saisis ; 0 kg convient aux exercices au poids du corps et ne produit pas de volume de charge externe.

## Alimentation

**Alimentation** présente un journal par date, les calories et les protéines, glucides et lipides par rapport aux objectifs personnels.

- Les objectifs sont saisis librement. Le choix maintien / prise de masse / perte de poids décrit l'intention et ne calcule pas une prescription.
- Les valeurs initiales sont des exemples modifiables : 2 600 kcal, P 150 g, G 320 g, L 80 g.
- Ajouter un aliment en g, ml ou pièces, ou une recette en portions.
- Les valeurs de référence sont définies pour 100 unités. En unité pièce, elles doivent donc correspondre à 100 pièces.
- Corriger une consommation ajuste ses valeurs proportionnellement. Modifier ensuite une fiche aliment ou une recette ne réécrit pas le journal passé.
- Les menus prévus restent séparés des repas consommés. **Consommé → journal** ajoute une copie du repas à sa date, avec protection contre l'ajout répété du même repas planifié.

## Recettes et préférences

La base initiale comprend quelques aliments et trois recettes éditables : œufs et pain complet, poulet coco avec riz et courgettes, chili avec riz.

Les valeurs sont indicatives et doivent être adaptées aux emballages. Les viandes et féculents de la base sont pesés crus. Les ingrédients correspondent à la recette complète ; le nombre de portions détermine les valeurs d'une portion.

Le sans lactose est activé par défaut. Il filtre la saisie des aliments/recettes et les suggestions ; la bibliothèque reste consultable. Chaque aliment dispose d'une déclaration sans lactose à vérifier lors de sa création. Une recette déjà planifiée devenue incompatible reste visible et signalée.

Réutiliser une même fiche aliment assure le regroupement des courses. Les doublons de nom et unité sont refusés.

## Menus, courses et batch cooking

- Afficher une période de 1, 3, 5, 7 ou 14 jours.
- Créer une proposition tenant compte des calories, des macros, du type de repas et du sans lactose.
- L'algorithme choisit parmi les recettes compatibles et ajuste leurs portions. Il cherche à se rapprocher des objectifs ; il ne garantit pas de les atteindre exactement. La répartition énergétique visée est de 25 % au petit-déjeuner et 37,5 % pour chacun des deux autres repas, avec une marge de 25 %.
- Relire la proposition avant de l'ajouter. Les repas existants sont conservés ; une nouvelle proposition ajoute des entrées.
- Le batch cooking répète les recettes sur le nombre de jours choisi (3 par défaut, de 1 à 14).
- Modifier les portions d'un repas recalcule les macros et les ingrédients.
- **Courses** ouvre directement les ingrédients regroupés par aliment et rayon, avec leurs quantités totales pour la période.
- Une case cochée est conservée pour le même aliment, la même période et la même quantité. Un changement de période ou de quantité la remet à décocher.
- Le récapitulatif de préparation additionne les portions à préparer pour chaque recette.

Les suggestions sont calculées localement. Il n'y a ni service nutrition externe, ni création de menus par un modèle distant, ni synchronisation mobile dans ce module.

## Données et validation

Les données sont stockées dans `Data.sportNutrition`, au sein du profil actif. Les anciennes sauvegardes reçoivent un module initialisé. Les exports JSON, sauvegardes complètes et restaurations incluent les séances, aliments, recettes, menus, journal et cases de courses.

Les modifications utilisent le mécanisme transactionnel existant : une validation ou un enregistrement en échec rétablit l'état précédent.

Compilation : `./build.ps1`.

Tests ciblés : `release/MonEspace.exe --sport-self-test DOSSIER_DE_TEST`.

Les 13 suites de `scripts/test.ps1` incluent cette nouvelle suite. Elle vérifie les calculs, filtres, sauvegardes, migrations, historique, cases de courses et les contrôles WPF réels, puis produit des captures. La CI Windows Server utilise `-SkipNativePdf` ; le moteur PDF natif reste un contrôle Windows desktop séparé. Les tests de dialogues ne remplacent pas un essai tactile ou une manipulation physique du bureau.
