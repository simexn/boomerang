<script lang="ts">
    import { goto } from '$app/navigation';
    import { fetchAdvancedUserInfo, type AdvancedUser } from '$lib/handlers/adminHandler';
    import { handleSearchUser } from '$lib/handlers/userHandler';
    import { updateUserInfo, userStore } from '$lib/stores/userInfoStore';
    import { onMount } from 'svelte';
    let ready = false;
    let findUserInput:string = '';
    let findUserError:string = '';
    let user: AdvancedUser;
    let newPassword: string = '';

    onMount(async () => {
        console.log($userStore?.isAdmin)
        await updateUserInfo();
        if($userStore?.isAdmin === false){
            await goto('/account');
        }

        ready = true;
    });
    async function findUser(){
        user = await fetchAdvancedUserInfo(findUserInput);
        console.log(user);
        if(user.id === 0){
            findUserError = 'User not found';
            user = {} as AdvancedUser;
        }

    }
    async function saveChanges(){
        
    }
    async function makeAdmin(){

    }
    async function deleteUser(){}
</script>
{#if ready}
<main>
    <div class="container">
    <h1>Администрация</h1>

    <form class="search-form">
        <label for="userId">Идентификатор на потребител:</label>
        <input type="text" id="userId" bind:value={findUserInput} />

        <button type="submit" on:click={() => findUser()}>Намери потребител</button>
    </form>

    {#if user}
        <div class="user-details">
            <div class="user-details-header">
                <h2>Детайли за потребителя</h2>
                <button class="delete-button" on:click={() => deleteUser()}>Изтрий потребител</button>
            </div>
            <label>Идентификатор на потребителя: <input type="text" bind:value={user.id} disabled /></label>
            <label>Потребителско име: <input type="text" bind:value={user.username} /></label>
            <label>Имейл: <input type="text" bind:value={user.email} /></label>
            <label>Дата на създаване на акаунт: <input type="text" bind:value={user.accountCreatedDate} disabled /></label>
            <label>Дата на раждане: <input type="text" bind:value={user.birthDate} /></label>
            <label>Пол: <input type="text" bind:value={user.pronouns} /></label>
            <label>Директория на профилна снимка: <input type="text" bind:value={user.profilePictureUrl} /></label>
            {#if user.IsAdmin}
                <label>Админ на проекта? Да.</label>
            {:else}
                <div class="admin-control">
                    <label>Админ на проекта? Не.</label>
                    <button on:click={() => makeAdmin()}>Направи админ</button>
                </div>
            {/if}
            <label>Нова парола: <input type="password" bind:value={newPassword} /></label>

            <button on:click={() => saveChanges()}>Запази промените</button>
        </div>
    {/if}
    </div>

</main>
{/if}

<style>
    .admin-control {
        display: flex;
        align-items: center;
        gap: 10px;
    }
    .container{
        width: 80%;
        margin: 1rem auto;
    }
    main {
        width: 100%;

        font-family: Arial, sans-serif;
        overflow: auto;
    }

    .search-form {
        margin-bottom: 2em;
    }

    .search-form label,
    .user-details label {
        display: block;
        margin-bottom: 0.5em;
    }

    .search-form input,
    .user-details input {
        width: 100%;
        padding: 0.5em;
        margin-bottom: 1em;
    }

    .search-form button,
    .user-details button {
        padding: 0.5em 1em;
        background-color: #007BFF;
        color: white;
        border: none;
        cursor: pointer;
    }

    .search-form button:hover,
    .user-details button:hover {
        background-color: #0056b3;
    }

    .user-details {
        background-color: #f8f9fa;
        padding: 2em;
        border-radius: 0.25em;
    }

    .user-details h2 {
        margin-bottom: 1em;
    }
    .user-details-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .delete-button {
        padding: 0.5em 1em;
        background-color: #dc3545 !important;
        color: white;
        border: none;
        cursor: pointer;
        margin-bottom: 2em;
    }

    .delete-button:hover {
        background-color: #c82333;
    }
</style>