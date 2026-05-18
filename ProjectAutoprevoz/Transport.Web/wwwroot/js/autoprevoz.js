window.autoprevoz = {
    login: async function (email, password) {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify({ email: email, password: password })
            });

            if (response.ok) {
                const data = await response.json();
                return {
                    Success: true,
                    ConnectionString: data.connectionString || '',
                    NazivFirme: data.nazivFirme || '',
                    IdKorisnika: data.idKorisnika || 0,
                    Privilegija: data.privilegija || 0,
                    Ime: data.ime || '',
                    Greska: null
                };
            }

            let poruka = 'Neispravni podaci za prijavu.';
            try {
                const err = await response.json();
                if (err && err.poruka) poruka = err.poruka;
            } catch (_) { }
            return { Success: false, Greska: poruka };

        } catch (_) {
            return { Success: false, Greska: 'Greška pri povezivanju sa serverom.' };
        }
    }
};
