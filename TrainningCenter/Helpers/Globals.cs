using TrainingCenter_BusinessLayer;

namespace TrainningCenter.Helpers
{
    public static class Globals
    {
        public static User CurrentUser { get; set; }
        public static bool isLoggedIn => CurrentUser != null;
        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
