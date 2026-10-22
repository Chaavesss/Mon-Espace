Mon espace 1.8 — instructions Ganymède dans le compagnon Dofus.

Le parcours Tougli et les validations existantes sont conservés. Les extraits Ganymède correspondant à la quête sont affichés dans la page Compagnon Dofus et sa vue compacte.

- Association par nom exact normalisé, avec vérification de l’identifiant du jeu quand il est disponible.
- Choix explicite si plusieurs guides correspondent.
- Navigation des sous-instructions partagée entre page et overlay, enregistrée par personnage sans avancer ni valider la quête Tougli.
- Chargement des guides principaux français ou association d’un guide public précis par URL / numéro.
- Coordonnées, copie /travel, attribution et lien vers l’étape source.
- Mode discret et sauvegardes incluent le suivi ; les guides restent un cache local, non distribués dans les sauvegardes ou le paquet.

Ouvrir **Tous mes outils → Compagnon Dofus**, charger le parcours Tougli puis **Charger les instructions Ganymède**. Le premier chargement peut prendre plusieurs minutes. Les quêtes sans extrait reconnu sont signalées. Illustrations et trajets communs hors des blocs de quête restent dans le guide source. Aucun compte Tougli / Ganymède n’est synchronisé. Les données du compagnon indépendant ne sont pas importées automatiquement.

Validation : 12 suites Windows natives, avec tests réels des boutons sur la page et dans l’overlay. PDF natif non retesté sur cette version : Windows Server exclut ce seul moteur des tests ; la validation CI le déclare explicitement et ne remplace pas une validation desktop complète.

Sources : Ganymède et auteurs des guides ; Tougli — Toufloux et Gligo. Aucun guide n’est inclus dans la distribution.
