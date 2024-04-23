<script lang="ts">
    import { onMount } from 'svelte';
    import Cropper from 'cropperjs';
    import 'cropperjs/dist/cropper.css';
    import { getToken } from "$lib/handlers/authHandler";
    import { updateUserInfo, userStore } from '$lib/stores/userInfoStore';
    import { handleUpdateInformation, validateConfirmPassword, validateEmail, validatePassword, validateUsername } from '$lib/handlers/accountHandler';
    const backendUrl = import.meta.env.VITE_BACKEND_URL;
    let selectedFile: any;
    let cropper: Cropper;
    let imageElement: HTMLImageElement;

    let infoEditable: boolean = false;

    let username: string = "";
    let email: string = "";

    let birthdate: string = "";
  let pronouns: string = "";
    let password: string = "";
    let newPassword: string = "";
    let confirmPassword: string = "";

    let fileName: string = "";
    let file: any;

    let usernameMessage: string = "";
    let emailMessage: string = "";
    let passwordMessage: string = "";
    let newPasswordMessage: string = "";
    let confirmPasswordMessage: string = "";

    onMount(() => {
        console.log("birthdate" + $userStore?.birthdate);
    });
    
    const handleFileChange = (event: Event) => {
    const inputElement = event.target as HTMLInputElement;
    if (inputElement.files) {
        file = inputElement.files[0];
        if (!file.type.startsWith('image/')) {
            alert('Моля изберете файл, който е снимка');
            return;
        }

        const reader = new FileReader();

        reader.onload = (e) => {
            if (e.target && typeof e.target.result === 'string') {
                imageElement.src = e.target.result;

                if (cropper) {
                    cropper.destroy();
                }

                cropper = new Cropper(imageElement, {
                    aspectRatio: 1,
                    viewMode: 1,
                    dragMode: 'move',
                    autoCropArea: 1,
                    cropBoxMovable: true,
                    cropBoxResizable: true,
                });
            }
        };

        reader.readAsDataURL(file);
    }
};

    const uploadFile = async () => {
        const canvas = cropper.getCroppedCanvas();
        canvas.toBlob(async (blob) => {
            if (blob) {
                let token = await getToken();
                let formData = new FormData();
                formData.append('file', blob, 'profile.png');

                let response = await fetch(`${backendUrl}/account/uploadPfp`, {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    },
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    console.log("File uploaded successfully");
                    await updateUserInfo();
                    
                }
                else{
                  const errorData = await response.text();
                    console.error("File upload failed:", errorData);
                }
            }
        });

    };

    async function updateInfo() {
        const formData = {
            username: username,
            email: email,
            birthdate: new Date(birthdate),
            pronouns: pronouns,
            password: password,
            newPassword: newPassword,
            confirmPassword: confirmPassword
        }

        usernameMessage = await validateUsername(username);
        emailMessage = await validateEmail(email);
        passwordMessage = await validatePassword(password);
        console.log(passwordMessage);
        newPasswordMessage = await validatePassword(newPassword);
        confirmPasswordMessage = await validateConfirmPassword(password, confirmPassword);

        if (usernameMessage.trim() === '' && emailMessage.trim() === '' && passwordMessage.trim() === '' && newPasswordMessage.trim() === '' && confirmPasswordMessage.trim() === '') {
            await handleUpdateInformation(formData);
        }
    }

    function resetFields() {
        
        username = "";
        email = "";
        password = "";
        newPassword = "";
        confirmPassword = "";
        usernameMessage = "";
        emailMessage = "";
        passwordMessage = "";
        newPasswordMessage = "";
        confirmPasswordMessage = "";
        infoEditable = false;
    }

    function isEditing() {
        infoEditable = !infoEditable;
        username = $userStore?.userName;
        email = $userStore?.email;
    }

</script>

<div class="container-fluid" style="overflow:auto; ">
    <h1>Редактиране на профил</h1>
    <hr>
    <div class="row">
        <div class="col-md-3">
            <div class="text-center">
                <img src={$userStore?.profilePictureUrl} style="border-radius:50%;"width="200px" height="200px" class="avatar img-circle" alt="avatar" bind:this={imageElement}>
                <h6>Профилна снимка</h6>
                <input accept="image/*" type="file" id="file" class="form-control" on:change="{handleFileChange}" hidden>
                <label for="file" class="file-label">
                  {#if !file}
                    Натиснете тук за да изберете снимка
                  {:else}
                    {file.name}
                  {/if}
                </label>
                {#if file}
                <button class="upload-photo-button" on:click="{uploadFile}">Качи снимка</button>
                {/if}
            </div>
        </div>
      
      <!-- edit form column -->
      <div class="col-md-9 personal-info">
        <h3>Лична информация</h3>
          <div class="info-wrap">
          <form class="form-horizontal" role="form">
            <div class="form-group">
              <label class="col-lg-3 control-label">Потребителско име:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="text" bind:value={username} placeholder="{$userStore?.userName}" disabled={!infoEditable}>
                  <p class="text-danger error-message"><i>{usernameMessage}</i></p>
              </div>
            </div>
            <div class="form-group">
              <label class="col-lg-3 control-label">Имейл:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="text" bind:value={email} placeholder="{$userStore?.email}" disabled={!infoEditable}>
                  <p class="text-danger error-message"><i>{emailMessage}</i></p>
              </div>
            </div>
            <div class="form-group">
              <label class="col-lg-3 control-label">Дата на раждане:</label>
              <div class="col-lg-8">
                      <input class="form-control date-input" type="date" value={$userStore?.birthdate?.toISOString().slice(0,10)} disabled={!infoEditable}>     
                      <p class="text-danger error-message"><i></i></p>
              </div>
          </div>
          <div class="form-group">
            <label class="col-lg-3 control-label">Пол:</label>
            <div class="mb-2 col-lg-8 ">
                <label>
                    <input type="radio" bind:group={pronouns} checked='{$userStore?.pronouns === "female"}' value="female" disabled={!infoEditable}> Жена
                </label>
                <label>
                    <input type="radio" bind:group={pronouns} checked='{$userStore?.pronouns === "male"}' value="male" disabled={!infoEditable}> Мъж
                </label>
                <label>
                    <input type="radio" bind:group={pronouns} checked='{$userStore?.pronouns === "other"}' value="other" disabled={!infoEditable}> Друг
                </label>
            </div>
        </div>
            <div class="form-group">
              <label class="col-lg-3 control-label">Парола:</label>
              <div class="col-lg-8">              
                  <input class="form-control" type="password" bind:value={password} placeholder="********" disabled={!infoEditable}>   
                  <p class="text-danger error-message"><i>{passwordMessage}</i></p>    
              </div>
              {#if infoEditable}
              <label class="col-lg-3 control-label">Нова парола:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="password" bind:value={newPassword} disabled={password == null}>
                  <p class="text-danger error-message"><i>{newPasswordMessage}</i></p>
              </div>
              <label class="col-lg-3 control-label">Потвърди паролата:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="password" bind:value={confirmPassword} disabled={password == null}>
                  <p class="text-danger error-message"><i>{confirmPasswordMessage}</i></p>
              </div>
              {/if}
            
              <div class="form-group">
                <label class="col-md-3 control-label"></label>
                <div class="col-md-8 center-inputs">
                    <input type="button" class="btn btn-primary" on:click={() => infoEditable ? updateInfo() : isEditing()} value={infoEditable ? 'Актуализирай информация' : 'Редактирай информация'}>
                    <span></span>
                    {#if infoEditable}
                      <input type="reset" class="btn btn-default" value="Отказ" on:click={resetFields}>
                    {/if}
                </div>
            </div>
          </form>
        </div>
      </div>
  </div>
</div>
<hr>

<style>
     .file-label {

        padding: 10px 20px;
        background-color: var(--prim-tp);
        color: white;
        cursor: pointer;
        border-radius: 5px;
        text-align: center;
        margin-bottom: 10px;
    }

    .file-label:hover {
        background-color: var(--hover-dark);
    }
    .upload-photo-button{
        border:none;
        padding: 10px 20px;
        background-color: var(--sec-tp);
        color: white;
        cursor: pointer;
        border-radius: 5px;
        text-align: center;
        margin-bottom: 10px;
    }
  .form-control{
    position: relative;
  }
    .date-input {
        -webkit-appearance: none;
        -moz-appearance: none;
        appearance: none;
    }
    .info-wrap{
      border: solid 1px;
      padding:3rem;
      max-width: 60rem;
    }

    .col-lg-8{
      width:100%;
    }
    .form-control{
      width: 100%;
    }

    .container-fluid{
        padding: 0 5rem 1rem 5rem;
    }
    @media only screen and (max-width: 600px) {
      .container-fluid{
        padding: 0 2rem 1rem 2rem;
      }
      .center-inputs{
        display: flex;
        flex-direction: column;
      }
    }
</style>