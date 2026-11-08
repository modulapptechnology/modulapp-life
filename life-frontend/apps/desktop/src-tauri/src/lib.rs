use keyring::Entry;

const SERVICE: &str = "com.modulapp.life";

fn entry(key: &str) -> Result<Entry, String> {
  Entry::new(SERVICE, key).map_err(|error| error.to_string())
}

#[tauri::command]
fn secure_get(key: String) -> Result<Option<String>, String> {
  match entry(&key)?.get_password() {
    Ok(value) => Ok(Some(value)),
    Err(keyring::Error::NoEntry) => Ok(None),
    Err(error) => Err(error.to_string()),
  }
}

#[tauri::command]
fn secure_set(key: String, value: String) -> Result<(), String> {
  entry(&key)?
    .set_password(&value)
    .map_err(|error| error.to_string())
}

#[tauri::command]
fn secure_delete(key: String) -> Result<(), String> {
  match entry(&key)?.delete_credential() {
    Ok(()) | Err(keyring::Error::NoEntry) => Ok(()),
    Err(error) => Err(error.to_string()),
  }
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
  tauri::Builder::default()
    .invoke_handler(tauri::generate_handler![
      secure_get,
      secure_set,
      secure_delete
    ])
    .run(tauri::generate_context!())
    .expect("error while building tauri application");
}
