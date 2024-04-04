<script lang="ts">
    import { onMount } from 'svelte';
    import Cropper from 'cropperjs';
    import 'cropperjs/dist/cropper.css';
    import { getToken } from "$lib/Handlers/authHandler";
    import { updateUserInfo, userStore } from '$lib/stores/userInfoStore';
    import { handleUpdateInformation, validateConfirmPassword, validateEmail, validatePassword, validateUsername } from '$lib/Handlers/accountHandler';
    const backendUrl = import.meta.env.VITE_BACKEND_URL;
    let selectedFile: any;
    let cropper: Cropper;
    let imageElement: HTMLImageElement;

    let infoEditable: boolean = false;

    let username: string = "";
    let email: string = "";
  
    let password: string = "";
    let newPassword: string = "";
    let confirmPassword: string = "";

    let usernameMessage: string = "";
    let emailMessage: string = "";
    let passwordMessage: string = "";
    let newPasswordMessage: string = "";
    let confirmPasswordMessage: string = "";
    

    onMount(() => {
        console.log("asd" + $userStore?.profilePictureUrl)
    });
    const handleFileChange = (event: Event) => {
        const inputElement = event.target as HTMLInputElement;
        if (inputElement.files) {
            const file = inputElement.files[0];
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
                formData.append('file', blob, 'profile.jpg');

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

<div class="container-fluid" style="overflow:auto; padding: 0 5rem 1rem 5rem;">
    <h1>Edit Profile</h1>
    <hr>
    <div class="row">
        <div class="col-md-3">
            <div class="text-center">
                <img src={$userStore?.profilePictureUrl} style="border-radius:50%;"width="200px" height="200px" class="avatar img-circle" alt="avatar" bind:this={imageElement}>
                <h6>Upload a different photo...</h6>
                <input type="file" class="form-control" on:change="{handleFileChange}">
                <button on:click="{uploadFile}">Upload</button>
            </div>
        </div>
      
      <!-- edit form column -->
      <div class="col-md-9 personal-info">
        <h3>Personal info</h3>
          <div class="info-wrap">
          <form class="form-horizontal" role="form">
            <div class="form-group">
              <label class="col-lg-3 control-label">Username:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="text" bind:value={username} placeholder="{$userStore?.userName}" disabled={!infoEditable}>
                  <p class="text-danger error-message"><i>{usernameMessage}</i></p>
              </div>
            </div>
            <div class="form-group">
              <label class="col-lg-3 control-label">E-mail:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="text" bind:value={email} placeholder="{$userStore?.email}" disabled={!infoEditable}>
                  <p class="text-danger error-message"><i>{emailMessage}</i></p>
              </div>
            </div>
            <div class="form-group">
              <label class="col-lg-3 control-label">Birthday:</label>
              <div class="col-lg-8">
                      <input class="form-control date-input" type="date" value="null" disabled={!infoEditable}>     
                      <p class="text-danger error-message"><i></i></p>
              </div>
          </div>
            <div class="form-group">
              <label class="col-lg-3 control-label">Password:</label>
              <div class="col-lg-8">              
                  <input class="form-control" type="password" bind:value={password} placeholder="12345678" disabled={!infoEditable}>   
                  <p class="text-danger error-message"><i>{passwordMessage}</i></p>    
              </div>
              {#if infoEditable}
              <label class="col-lg-3 control-label">New password:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="password" bind:value={newPassword} placeholder="12345678" disabled={password == null}>
                  <p class="text-danger error-message"><i>{newPasswordMessage}</i></p>
              </div>
              <label class="col-lg-3 control-label">Confirm password:</label>
              <div class="col-lg-8">
                  <input class="form-control" type="password" bind:value={confirmPassword} placeholder="12345678" disabled={password == null}>
                  <p class="text-danger error-message"><i>{confirmPasswordMessage}</i></p>
              </div>
              {/if}
            
              <div class="form-group">
                <label class="col-md-3 control-label"></label>
                <div class="col-md-8">
                    <input type="button" class="btn btn-primary" on:click={() => infoEditable ? updateInfo() : isEditing()} value={infoEditable ? 'Update information' : 'Edit information'}>
                    <span></span>
                    {#if infoEditable}
                      <input type="reset" class="btn btn-default" value="Cancel" on:click={resetFields}>
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
      .input-icon-container {
        position: relative;
    }
  .form-control{
    position: relative;
  }
  .fa-edit {
        position: absolute;
        top: 0.4rem;
        right: 0.5rem;
        /* Adjust these values as needed */
        padding: 0.25rem;
        cursor: pointer;
    }

    .date-input-container {
        position: relative;
    }
    .date-input-container::before {
        content: "\f073"; /* This is the Unicode for the Font Awesome calendar icon */
        font-family: "Font Awesome 5 Free"; /* Adjust according to the icon font you're using */
        font-weight: 900; /* Adjust according to the icon font you're using */
        position: absolute;
        right: 2.5rem; /* Adjust as needed */
        top: 0.4rem; /* Adjust as needed */
        pointer-events: none;
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
</style>