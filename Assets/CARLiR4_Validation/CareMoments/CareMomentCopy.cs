namespace CarliSenseXR.CareMoments
{
    // Public copy table. No AI inference, analytics, diagnosis, audio, or network calls.
    public static class CareMomentCopy
    {
        public struct StepContent
        {
            public readonly string title, body, comfort, privacy, action;
            public StepContent(string t, string b, string c, string p, string a)
            { title = t; body = b; comfort = c; privacy = p; action = a; }
        }

        private static readonly StepContent[][] Content =
        {
            new[] // English
            {
                new StepContent("One little moment", "What did you actually notice today? Start with something you saw or heard.", "Noticing is a form of care.", "Your observation stays in this draft until you choose to export.", "Choose an observation"),
                new StepContent("The world around them", "Was it after a walk, meal, playtime, or rest? Add the context you know.", "Small details help you remember, without jumping to conclusions.", "Context is caregiver-provided, not a diagnosis.", "Add context"),
                new StepContent("A kind next step", "Choose a gentle response you can actually do, like offering a quiet break or checking their water.", "Care can be simple and steady.", "This app does not tell you what your pet feels or diagnose illness.", "Choose your response"),
                new StepContent("Your choice matters", "Hold to consent only if you want this observation saved to a local journal.", "You can say not now. Your bond is not measured by saved data.", "No export before explicit consent. STOP discards the current draft.", "Hold to consent"),
                new StepContent("A memory you control", "Review your notes and choose Export when you are ready.", "These moments belong to you and your companion.", "Export creates a local file. STOP does not erase previously exported files.", "Review and export"),
                new StepContent("Thank you for noticing", "A small moment has been recorded. Return to your pet, not just the screen.", "The relationship happens beyond this app.", "Previously exported files remain on this device until removed separately.", "Start a new moment")
            },
            new[] // Chinese
            {
                new StepContent("今天的小小瞬间", "今天你亲眼看见或听见了什么？从一个真实的行为开始。", "愿意停下来观察，就是一种陪伴。", "这份观察会先留在草稿中，只有你选择导出才会保存为日志文件。", "选择观察"),
                new StepContent("它身边的世界", "事情发生在散步、吃饭、玩耍，还是休息之后？记录你确实知道的背景。", "不急着下结论，也能更懂得照顾它。", "背景由主人提供，不代表诊断。", "补充背景"),
                new StepContent("一个温柔的下一步", "选择你愿意实际做的小行动，例如提供安静休息的空间或检查饮水。", "照顾不一定轰轰烈烈，稳定就很珍贵。", "我们不猜测宠物内心，也不提供疾病诊断。", "选择回应"),
                new StepContent("决定权在你手上", "只有当你愿意保存这段观察时，才长按授权。", "今天不保存也没关系；爱不需要靠数据证明。", "未经明确同意不得导出；STOP 只清除当前草稿。", "长按授权"),
                new StepContent("一份由你保管的回忆", "先确认记录，再由你决定是否导出。", "这些日常片段属于你和你的伙伴。", "导出会在本机建立文件；STOP 不会删除此前已保存的文件。", "检查并导出"),
                new StepContent("谢谢你认真看见它", "这个片段已经记录。现在也别忘了回到它身边。", "真正的陪伴，发生在屏幕之外。", "以前导出的文件仍保存在设备中，须另行管理。", "开始新的瞬间")
            },
            new[] // Malay
            {
                new StepContent("Detik kecil hari ini", "Apakah yang benar-benar anda lihat atau dengar hari ini? Mulakan dengan pemerhatian nyata.", "Memberi perhatian juga satu bentuk kasih sayang.", "Pemerhatian kekal sebagai draf sehingga anda memilih untuk mengeksport.", "Pilih pemerhatian"),
                new StepContent("Dunia di sekelilingnya", "Adakah ia berlaku selepas berjalan, makan, bermain atau berehat? Catat konteks yang anda tahu.", "Butiran kecil membantu tanpa membuat andaian.", "Konteks diberi penjaga dan bukan diagnosis.", "Tambah konteks"),
                new StepContent("Langkah kecil yang baik", "Pilih tindakan lembut yang boleh dibuat, seperti menyediakan ruang rehat atau memeriksa air minuman.", "Penjagaan yang konsisten amat bermakna.", "Aplikasi ini tidak meneka perasaan haiwan atau mendiagnosis penyakit.", "Pilih tindakan"),
                new StepContent("Pilihan di tangan anda", "Tekan dan tahan untuk bersetuju hanya jika anda mahu menyimpan pemerhatian ke jurnal setempat.", "Tidak menyimpan juga tidak mengapa.", "Tiada eksport tanpa persetujuan jelas. STOP membuang draf semasa.", "Tahan untuk setuju"),
                new StepContent("Kenangan dalam kawalan anda", "Semak catatan dan pilih Eksport apabila bersedia.", "Detik ini milik anda dan teman kesayangan.", "Eksport mencipta fail setempat. STOP tidak memadam fail yang pernah dieksport.", "Semak dan eksport"),
                new StepContent("Terima kasih kerana prihatin", "Detik ini telah direkodkan. Kembalilah meluangkan masa bersamanya.", "Hubungan sebenar berlaku di luar skrin.", "Fail terdahulu kekal pada peranti sehingga dipadam secara berasingan.", "Mulakan detik baharu")
            }
        };

        private static readonly string[][] Observations =
        {
            new[] {"Caregiver observed: pet rested in a familiar spot.", "Caregiver observed: pet approached when called.", "Caregiver observed: pet engaged with a toy.", "Caregiver observed: pet drank from its water bowl."},
            new[] {"主人观察到：宠物在熟悉的位置休息。", "主人观察到：呼唤后宠物走近。", "主人观察到：宠物与玩具互动。", "主人观察到：宠物从水碗饮水。"},
            new[] {"Penjaga memerhati: haiwan berehat di tempat biasa.", "Penjaga memerhati: haiwan mendekat apabila dipanggil.", "Penjaga memerhati: haiwan bermain dengan mainan.", "Penjaga memerhati: haiwan minum dari mangkuk air."}
        };

        public static StepContent ForStep(int step, CareLanguage language)
        {
            int l = (int)language;
            if (l < 0 || l >= Content.Length) l = 0;
            if (step < 0 || step > 5) step = 0;
            return Content[l][step];
        }
        public static string ObservationFact(int index, CareLanguage language)
        {
            int l = (int)language;
            if (l < 0 || l >= Observations.Length) l = 0;
            return index >= 0 && index < 4 ? Observations[l][index] : "";
        }
        public static string SelectionPrompt(CareLanguage language)
        {
            switch (language)
            {
                case CareLanguage.Chinese: return "先选一个你实际观察到的行为";
                case CareLanguage.Malay: return "Pilih tingkah laku yang anda benar-benar perhatikan";
                default: return "Select something you actually observed";
            }
        }
    }
}
