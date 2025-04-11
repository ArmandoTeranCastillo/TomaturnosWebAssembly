// Variable global para mantener una referencia al objeto Audio
window.currentAudio = null;

// Crear o abrir la base de datos de IndexedDB
function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('AudioDatabase', 1);

        request.onupgradeneeded = function(event) {
            const db = event.target.result;
            if (!db.objectStoreNames.contains('audios')) {
                // Cambiar la clave a "id" en lugar de "fileName"
                db.createObjectStore('audios', { keyPath: 'id' });
            }
        };

        request.onsuccess = function(event) {
            resolve(event.target.result);
        };

        request.onerror = function(event) {
            reject('Error opening IndexedDB: ' + event.target.errorCode);
        };
    });
}

// Guardar el audio en IndexedDB
function saveAudioToIndexedDB(id, name, audioBlob) {
    return openDatabase().then(db => {
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['audios'], 'readwrite');
            const store = transaction.objectStore('audios');
            store.put({ id: id, audioBlob: audioBlob });

            transaction.oncomplete = function() {
                console.log('Audio guardado en IndexedDB: ' + id + ' - ' + name);
                resolve();
            };

            transaction.onerror = function(event) {
                console.error('Error saving audio to IndexedDB: ', event.target.errorCode);
                reject('Error saving audio to IndexedDB: ' + event.target.errorCode);
            };
        });
    });
}

// Obtener el audio desde IndexedDB
function getAudioFromIndexedDB(id, name) {
    return openDatabase().then(db => {
        return new Promise((resolve, reject) => {
            const transaction = db.transaction(['audios'], 'readonly');
            const store = transaction.objectStore('audios');
            const request = store.get(id);

            request.onsuccess = function(event) {
                console.log('Audio encontrado en IndexedDB: ' + id + ' - ' + name);
                resolve(event.target.result ? event.target.result.audioBlob : null);
            };

            request.onerror = function(event) {
                console.error('Error fetching audio from IndexedDB: ', event.target.errorCode);
                reject('Error fetching audio from IndexedDB: ' + event.target.errorCode);
            };
        });
    });
}

// Reproducir el audio desde IndexedDB o desde un nuevo archivo base64
function playAudio(id, name, base64Audio) {
    // Detener el audio actual si está en reproducción
    stopAudio();

    if (!base64Audio) {
        console.error('No se encontró el audio base64 para reproducir.');
        return;
    }

    getAudioFromIndexedDB(id, name).then(cachedAudio => {
        if (cachedAudio) {
            // Reproduce el audio desde IndexedDB sin volver a guardarlo
            console.log("Reproduciendo audio desde IndexedDB: " + id + " - " + name);
            window.currentAudio = new Audio(URL.createObjectURL(cachedAudio));
            window.currentAudio.play().catch(e => {
                console.error('Error reproduciendo audio en caché:', e);
            });
        } else {
            // Convertir base64 a Blob y reproducir el audio, luego guardarlo
            console.log("Reproduciendo y almacenando nuevo audio: " + id + " - " + name);
            const audioBlob = base64ToBlob(base64Audio, 'audio/mp3');
            window.currentAudio = new Audio(URL.createObjectURL(audioBlob));
            window.currentAudio.play().then(() => {
                // Guarda solo el audio nuevo en IndexedDB
                saveAudioToIndexedDB(id, name, audioBlob);
            }).catch(e => {
                console.error('Error reproduciendo nuevo audio:', e);
            });
        }
    }).catch(error => {
        console.error('Error buscando el audio en IndexedDB: ', error);
    });
}

//Reproducir audio desde un archivo base64
function playAudioWithoutSave(base64Audio) {
    // Detener el audio actual si está en reproducción
    stopAudio();

    if (!base64Audio) {
        console.error('No se encontró el audio base64 para reproducir.');
        return;
    }

    // Convertir base64 a Blob y reproducir el audio
    console.log("Reproduciendo audio");
    const audioBlob = base64ToBlob(base64Audio, 'audio/mp3');
    window.currentAudio = new Audio(URL.createObjectURL(audioBlob));
    window.currentAudio.play().catch(e => {
        console.error('Error reproduciendo audio:', e);
    });
}

// Convertir base64 a Blob
function base64ToBlob(base64, mimeType) {
    const byteChars = atob(base64);
    const byteNumbers = new Array(byteChars.length);
    for (let i = 0; i < byteChars.length; i++) {
        byteNumbers[i] = byteChars.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
}

// Detener el audio actual
window.stopAudio = function() {
    if (window.currentAudio) {
        window.currentAudio.pause();
        window.currentAudio.currentTime = 0; // Reiniciar el tiempo a 0
    }
}

// Animación de reinicio para el turno actual
window.addTurnoActualAnimation = function() {
    const turnoActualContainer = document.querySelector('.turno-actual-container');
    if (turnoActualContainer) {
        turnoActualContainer.classList.remove('animate-turno-actual');  // Eliminar clase si ya está presente
        void turnoActualContainer.offsetWidth;  // Forzar el reflow para reiniciar la animación
        turnoActualContainer.classList.add('animate-turno-actual');     // Agregar la clase para la animación
    }
}