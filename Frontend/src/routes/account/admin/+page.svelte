<script lang="ts">
    import { handleDeleteUser, handleFetchUserModerate, handleUpdateUser } from "$lib/handlers/adminHandler";
    import type  {UserModerate}  from "$lib/handlers/adminHandler";
    import { onMount } from 'svelte';
    import { userStore, updateUserInfo } from '$lib/stores/userInfoStore';


    let identificator = '';
    let newPassword = "";

    let user: UserModerate;
    let isAdmin = false;

    onMount(async () => {
        await updateUserInfo();
        const currentUser = $userStore;
        isAdmin = currentUser.isAdmin;
        if (!isAdmin) {
           window.location.href = '/chat/home';
        }
    });
  
    async function findUser() {
        user = await handleFetchUserModerate(identificator);
        console.log(user.username)
    }
  
    function makeAdmin() {
      user.isAdmin = true;
    }
  
    async function deleteUser() {
      await handleDeleteUser(user.id)
    }
  
    async function saveChanges() {
        const formData = { 
            id: user.id,
            username : user.username, 
            email : user.email, 
            password: newPassword, 
            birthDate: new Date(user.birthDate), 
            pronouns: user.pronouns,
            profilePictureUrl: user.profilePictureUrl,
            newPassword: newPassword,
            isAdmin: user.isAdmin
        };

        await handleUpdateUser(formData);
        
      
    }
  </script>
  
 
  <div class="container mt-4">
    <h1>Администрация</h1>
    <div class="mb-3">
      <label for="userIdInput" class="form-label">Идентификатор на потребителя:</label>
      <div class="input-group">
        <input
          type="text"
          class="form-control"
          id="userIdInput"
          bind:value={identificator}
        />
        <button class="btn btn-primary" on:click={findUser}>Намери потребител</button>
      </div>
    </div>
    {#if user}
    <div class="details-container">
      <h2>Детайли за потребителя</h2>
      <div class="mb-3">
        <label class="form-label">Идентификатор на потребителя:</label>
        <input type="text" class="form-control" bind:value={user.id} readonly />
      </div>
      <div class="mb-3">
        <label class="form-label">Потребителско име:</label>
        <input type="text" class="form-control" bind:value={user.username} />
      </div>
      <div class="mb-3">
        <label class="form-label">Имейл:</label>
        <input type="email" class="form-control" bind:value={user.email} />
      </div>
      <div class="mb-3">
        <label class="form-label">Дата на създаване на акаунт:</label>
        <input type="text" class="form-control" bind:value={user.accountCreatedDate} readonly />
      </div>
      <div class="mb-3">
        <label class="form-label">Дата на раждане:</label>
        <input type="text" class="form-control" bind:value={user.birthDate} />
      </div>
      <div class="mb-3">
        <label class="form-label">Пол:</label>
        <input type="text" class="form-control" bind:value={user.pronouns} />
      </div>
      <div class="mb-3">
        <label class="form-label">Директория на профилна снимка:</label>
        <input type="text" class="form-control" bind:value={user.profilePictureUrl} />
      </div>
      <div class="mb-3">
        <label class="form-label">Админ на проекта? {user.isAdmin ? 'Да' : 'Не'}</label>
        {#if !user.isAdmin}
          <button class="btn btn-blue" on:click={makeAdmin}>Направи админ</button>
        {/if}
      </div>
      <div class="mb-3">
        <label class="form-label">Нова парола:</label>
        <input type="password" class="form-control" bind:value={newPassword} />
      </div>
      <button class="btn btn-primary" on:click={saveChanges}>Запази промените</button>
      <button class="btn btn-red mt-2" on:click={deleteUser}>Изтрий потребител</button>
    </div>
    {/if}
  </div>
  
  <style>
    .container {
      max-width: 800px;
      margin: auto;
      padding: 20px;
      height: 70vh;
    }
    .details-container {
      max-height: 100%; /* Adjust the height as needed */
      overflow-y: auto;
      padding-bottom: 10px;
      border: 1px solid #ddd; /* Optional: Adds a border for visual distinction */
      padding: 10px; /* Optional: Adds some padding inside the container */
    }
    .btn-red {
      background-color: red;
      color: white;
    }
    .btn-blue {
      background-color: blue;
      color: white;
    }
    .form-control[readonly] {
      background-color: #e9ecef;
    }
  </style>
  