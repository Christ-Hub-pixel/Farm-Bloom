using System;
using System.Collections.Generic;
using UnityEngine;

namespace FarmBloom.Core
{
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance => _instance;

        public event Action OnLanguageChanged;

        private string _currentLanguage = "fr";
        public string CurrentLanguage => _currentLanguage;

        private readonly Dictionary<string, Dictionary<string, string>> _dictionary = new Dictionary<string, Dictionary<string, string>>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitTranslations();
            if (SaveManager.Instance?.Data != null)
            {
                _currentLanguage = SaveManager.Instance.Data.language;
            }
        }

        public void SetLanguage(string lang)
        {
            _currentLanguage = lang;
            if (SaveManager.Instance?.Data != null)
            {
                SaveManager.Instance.Data.language = lang;
                SaveManager.Instance.Save();
            }
            OnLanguageChanged?.Invoke();
        }

        public string Get(string key, string fallback = "")
        {
            if (_dictionary.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(_currentLanguage, out var text))
                {
                    return text;
                }
                if (translations.TryGetValue("en", out var enText))
                {
                    return enText;
                }
            }
            return string.IsNullOrEmpty(fallback) ? key : fallback;
        }

        private void InitTranslations()
        {
            Add("app_title", "Farm Bloom", "Farm Bloom", "Farm Bloom", "Farm Bloom");
            Add("tagline", "RÉCOLTE • CONSTRUIS • PROGRESSE", "HARVEST • BUILD • PROGRESS", "COSECHA • CONSTRUYE • PROGRESA", "COLHA • CONSTRUA • PROGRIDA");
            Add("play", "JOUER", "PLAY", "JUGAR", "JOGAR");
            Add("login", "Connexion", "Login", "Iniciar sesión", "Entrar");
            Add("guest", "Invité", "Guest", "Invitado", "Convidado");
            Add("create_account", "Créer un compte", "Create Account", "Crear cuenta", "Criar conta");
            Add("username", "Nom d'utilisateur", "Username", "Usuario", "Nome de usuário");
            Add("email", "Email", "Email", "Correo", "E-mail");
            Add("password", "Mot de passe", "Password", "Contraseña", "Senha");
            Add("continue", "Continuer", "Continue", "Continuar", "Continuar");
            Add("retry", "Réessayer", "Retry", "Reintentar", "Tentar novamente");
            Add("settings", "Paramètres", "Settings", "Ajustes", "Configurações");
            Add("moves", "Coups", "Moves", "Movimientos", "Jogadas");
            Add("score", "Score", "Score", "Puntuación", "Pontuação");
            Add("victory", "Bravo !", "Awesome!", "¡Genial!", "Muito bem!");
            Add("defeat", "Manque de coups !", "Out of moves!", "¡Sin movimientos!", "Fim de jogadas!");
            Add("combo", "Combo génial !", "Super Combo!", "¡Combo genial!", "Combo incrível!");
            Add("objective", "Objectif", "Objective", "Objetivo", "Objetivo");
            Add("daily_reward", "Récompense quotidienne", "Daily Reward", "Recompensa diaria", "Recompensa diária");
            Add("spin_wheel", "Roue de récompenses", "Lucky Wheel", "Rueda de la suerte", "Roda da sorte");
            Add("spin", "TOURNER", "SPIN", "GIRAR", "GIRAR");
            Add("free_spin", "Tour gratuit disponible !", "Free spin available!", "¡Giro gratis disponible!", "Giro grátis disponível!");
            Add("barn", "Grange", "Barn", "Granero", "Celeiro");
            Add("capacity", "Capacité", "Capacity", "Capacidad", "Capacidade");
            Add("upgrade", "Améliorer", "Upgrade", "Mejorar", "Melhorar");
            Add("harvest", "Récolter", "Harvest", "Cosechar", "Colher");
            Add("plant", "Planter", "Plant", "Sembrar", "Plantar");
            Add("animals", "Mes animaux", "My Animals", "Mis animales", "Meus animais");
            Add("chicken", "Poulet", "Chicken", "Pollo", "Galinha");
            Add("cow", "Vache", "Cow", "Vaca", "Vaca");
            Add("goat", "Chèvre", "Goat", "Cabra", "Cabra");
            Add("bee", "Abeille", "Bee", "Abeja", "Abelha");
            Add("shop", "Boutique", "Shop", "Tienda", "Loja");
            Add("coins", "Pièces", "Coins", "Monedas", "Moedas");
            Add("gems", "Gemmes", "Gems", "Gemas", "Gemas");
            Add("packs", "Packs", "Packs", "Paquetes", "Pacotes");
            Add("pass", "Pass saisonnier", "Season Pass", "Pase de temporada", "Passe de temporada");
            Add("vault", "Coffre-fort", "Vault", "Caja fuerte", "Cofre");
            Add("vip", "Abonnement VIP", "VIP Pass", "Pase VIP", "Assinatura VIP");
            Add("profile", "Profil joueur", "Player Profile", "Perfil de jugador", "Perfil do jogador");
            Add("avatar", "Choisir un avatar", "Choose Avatar", "Elegir avatar", "Escolher avatar");
            Add("friends", "Mes amis", "Friends", "Amigos", "Amigos");
            Add("leaderboard", "Classement mondial", "Leaderboard", "Clasificación", "Classificação");
            Add("daily_quests", "Défis quotidiens", "Daily Quests", "Desafíos diarios", "Missões diárias");
            Add("club", "Club des fermiers", "Farmers Club", "Club granjero", "Clube de fazendeiros");
            Add("messages", "Boîte de réception", "Inbox", "Buzón", "Mensagens");
            Add("sound", "Effets sonores", "Sound Effects", "Efectos", "Efeitos sonoros");
            Add("music", "Musique", "Music", "Música", "Música");
            Add("vibration", "Vibration", "Vibration", "Vibración", "Vibração");
        }

        private void Add(string key, string fr, string en, string es, string pt)
        {
            _dictionary[key] = new Dictionary<string, string>
            {
                { "fr", fr },
                { "en", en },
                { "es", es },
                { "pt", pt }
            };
        }
    }
}
